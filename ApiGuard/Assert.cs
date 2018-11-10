using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Exceptions;
using ApiGuard.Models;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace ApiGuard
{
    public static class Assert
    {
        public static async Task HasNotChanged(params Type[] types)
        {
            foreach (var type in types)
            {
                await HasNotChanged(type);
            }
        }

        public static async Task HasNotChanged(Type type)
        {
            // Get the path to the project
            var assemblyPath = type.Assembly.Location;
            var projectName = type.Assembly.GetName().Name;
            var bin = Path.DirectorySeparatorChar + "bin";
            var testProjectPath = assemblyPath.Substring(0, assemblyPath.IndexOf(bin, StringComparison.InvariantCultureIgnoreCase));
            var solutionPath = testProjectPath.Substring(0, testProjectPath.LastIndexOf(Path.DirectorySeparatorChar));
            var apiProjectPath = Path.Combine(solutionPath, projectName, $"{projectName}.csproj");

            // Load the project into Roslyn workspace
            var analyzerManager = new AnalyzerManager();
            var adHocProject = analyzerManager.GetProject(apiProjectPath);
            var workspace = adHocProject.GetWorkspace();
            var project = workspace.CurrentSolution.Projects.Single(x => x.Name == projectName);

            // Build the project
            var compilation = await project.GetCompilationAsync();

            // Get symbol for the type passed in
            var symbol = compilation.GetSymbolsWithName(x => x == type.Name).OfType<INamedTypeSymbol>().Single();
            var definingAssembly = symbol.ContainingAssembly;

            // For each method create an entry (Endpoint)
            // Each Endpoint contains the name of the method, return type and its arguments as well as targeted attributes
            // If the Type is a complex object, we repeat the process for that object but also include properties
            var endpoints = new List<Endpoint>();
            foreach (var methodSymbol in symbol.GetMembers().OfType<IMethodSymbol>().Where(x => x.CanBeReferencedByName))
            {
                var endpoint = new Endpoint
                {
                    MethodName = methodSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    ReturnType = new MyType(methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                    Attributes = methodSymbol.GetAttributes().Select(x => x.AttributeClass).ToList(),
                };

                foreach (var parameter in methodSymbol.Parameters)
                {
                    var param = new MyParameter(new MyType(parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)), parameter.Ordinal);

                    Fill(param, parameter, definingAssembly);
                    endpoint.Parameters.Add(param);
                }

                endpoints.Add(endpoint);
            }

            var api = new Api
            {
                TypeName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Endpoints = endpoints
            };

            // Once this entire tree is parsed, see if there is an existing tree document in the repository
            var documentPath = Path.Combine(solutionPath, projectName, $"api_{symbol.Name}.json");
            var hasExistingApiDocument = File.Exists(documentPath);

            // If there isn't, create one and mark as success
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            if (!hasExistingApiDocument)
            {
                File.WriteAllText(documentPath, JsonConvert.SerializeObject(api, Formatting.Indented, serializerSettings));
                return;
            }

            // If there is, load the existing document into memory
            Api existingApi;
            try
            {
                var existingApiJson = File.ReadAllText(documentPath);
                existingApi = JsonConvert.DeserializeObject<Api>(existingApiJson, serializerSettings);
            }
            catch (Exception e)
            {
                throw new FileNotFoundException("Unable to find or open the API file", e);
            }

            if (existingApi.TypeName != api.TypeName)
            {
                throw new ApiNotFoundException(existingApi.TypeName);
            }

            // Loop over the existing document elements
            foreach (var endpoint in existingApi.Endpoints)
            {
                // For each endpoint in there, find the corresponding endpoint in the new tree
                // Compare the basic endpoint data (name, return type, number of arguments)
                // If all are correct, compare the argument types in the order they were originally
                // If the type is a complex object, recurse into it and compare properties / methods
                var correspondingEndpoint = api.GetMatchingEndpoint(endpoint);
                if (correspondingEndpoint == null)
                {
                    throw new EndpointNotFoundException(endpoint, api.TypeName);
                }
            }


            // The exception to this is when we see a [BETA] or [OBSOLETE] attribute on it
            // Otherwise, return success

            // In a separate project, provide a [BETA] attribute and corresponding Roslyn analyzer so it shows a warning

            // Out of scope:
            // Protected modifier
            // Fields / Events / Delegates
            // Named arguments
            // Datamember attributes on complex objects
            // Automatic pass if there is a major version increase
        }

        private static void Fill(MyParameter param, IParameterSymbol parameterSymbol, IAssemblySymbol definingAssembly)
        {
            var parameterType = parameterSymbol.Type;
            if (!Equals(parameterType.ContainingAssembly.Name, definingAssembly.Name))
            {
                param.Type.Typename = parameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
            else
            {
                if (parameterType.IsValueType)
                {
                    param.Type.Typename = parameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
                else
                {
                    // The parameter is a custom object
                    // Extract properties and methods
                    var classType = (INamedTypeSymbol) parameterType;
                    Fill(param.Type, classType, definingAssembly);
                }
            }
        }

        private static void Fill(MyType type, INamedTypeSymbol complexObject, IAssemblySymbol definingAssembly)
        {
            var properties = complexObject.GetMembers().OfType<IPropertySymbol>().ToList();
            foreach (var property in properties)
            {
                var newElement = new MyProperty
                {
                    Name = property.Name,
                    Type = new MyType(property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                };

                type.NestedElements.Add(newElement);
                Fill(newElement, property, definingAssembly);
            }

            var methods = complexObject.GetMembers().OfType<IMethodSymbol>().Where(x => x.CanBeReferencedByName).ToList();
            foreach (var method in methods)
            {
                var newElement = new MyMethod(method.Name, new MyType(method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

                type.NestedElements.Add(newElement);
                Fill(newElement, method, definingAssembly);

                foreach (var methodParameter in method.Parameters)
                {
                    var newParameter = new MyParameter(new MyType(methodParameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)), methodParameter.Ordinal);

                    newElement.Parameters.Add(newParameter);
                    Fill(newParameter, methodParameter, definingAssembly);
                }
            }
        }

        private static void Fill(MyProperty property, IPropertySymbol propertySymbol, IAssemblySymbol definingAssembly)
        {
            var propertyType = propertySymbol.Type;
            if (!Equals(propertyType.ContainingAssembly, definingAssembly))
            {
                property.Type.Typename = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
            else
            {
                if (propertyType.IsValueType)
                {
                    property.Type.Typename = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
                else
                {
                    // The parameter is a custom object
                    // Extract properties and methods
                    var classType = (INamedTypeSymbol)propertyType;
                    Fill(property.Type, classType, definingAssembly);
                }
            }
        }

        private static void Fill(MyMethod method, IMethodSymbol methodSymbol, IAssemblySymbol definingAssembly)
        {
            var returnType = methodSymbol.ReturnType;
            if (!Equals(returnType.ContainingAssembly, definingAssembly))
            {
                method.ReturnType.Typename = returnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
            else
            {
                if (returnType.IsValueType)
                {
                    method.ReturnType.Typename = returnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
                else
                {
                    // The parameter is a custom object
                    // Extract properties and methods
                    var classType = (INamedTypeSymbol)returnType;
                    Fill(method.ReturnType, classType, definingAssembly);
                }
            }
        }
    }
}
