using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Domain.Strategies;
using ApiGuard.Models;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;

namespace ApiGuard.Domain
{
    internal class RoslynTypeLoader : ITypeLoader
    {
        private readonly IProjectResolver _projectResolver;

        public RoslynTypeLoader(IProjectResolver projectResolver)
        {
            _projectResolver = projectResolver;
        }

        public async Task<Api> LoadApi(Type type)
        {
            var projectInfo = _projectResolver.GetProjectInfo(type);
            var apiProjectPath = projectInfo.ProjectFilePath;

            // Load the project into Roslyn workspace
            var analyzerManager = new AnalyzerManager();
            var adHocProject = analyzerManager.GetProject(apiProjectPath);
            var workspace = adHocProject.GetWorkspace();
            var project = workspace.CurrentSolution.Projects.Single(x => x.Name == projectInfo.ProjectName);

            // Build the project
            var compilation = await project.GetCompilationAsync();

            // Get symbol for the type passed in
            var apiSymbol = compilation.GetSymbolsWithName(x => x == type.Name).OfType<INamedTypeSymbol>().Single();
            var definingAssembly = apiSymbol.ContainingAssembly;

            // For each method create an entry (Endpoint)
            // Each Endpoint contains the name of the method, return type and its arguments as well as targeted attributes
            // If the Type is a complex object, we repeat the process for that object but also include properties
            var endpoints = new List<MyMethod>();
            var depth = 0;
            foreach (var methodSymbol in apiSymbol.GetMembers().OfType<IMethodSymbol>().Where(x => x.CanBeReferencedByName))
            {
                var endpoint = new MyMethod(
                    methodSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    new MyType(methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), depth + 1))
                {
                    Attributes = methodSymbol.GetAttributes().Select(x => x.AttributeClass).Select(x => new MyType(x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), depth)).ToList(),
                    Depth = depth,
                };

                foreach (var parameter in methodSymbol.Parameters)
                {
                    var param = new MyParameter(new MyType(parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), depth + 1), parameter.Ordinal);
                    param.Depth = depth;
                    Fill(param, parameter, definingAssembly, 0);
                    endpoint.Parameters.Add(param);
                }

                endpoints.Add(endpoint);
            }

            var api = new Api(new BestGuessEndpointMatchingStrategy())
            {
                TypeName = apiSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Endpoints = endpoints
            };

            return api;
        }
        
        private static void Fill(MyType type, INamedTypeSymbol complexObject, IAssemblySymbol definingAssembly, int depth)
        {
            depth++;
            var properties = complexObject.GetMembers().OfType<IPropertySymbol>().ToList();
            foreach (var property in properties)
            {
                var newElement = new MyProperty
                {
                    Name = property.Name,
                    Type = new MyType(property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), depth + 1),
                    Depth = depth,
                    ParentTypeName = type.Typename
                };

                type.NestedElements.Add(newElement);
                Fill(newElement, property, definingAssembly, depth);
            }

            var methods = complexObject.GetMembers().OfType<IMethodSymbol>().Where(x => x.CanBeReferencedByName).ToList();
            foreach (var method in methods)
            {
                var newElement = new MyMethod(method.Name, new MyType(method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), depth + 1));
                newElement.Depth = depth;
                newElement.ParentTypeName = type.Typename;
                type.NestedElements.Add(newElement);
                Fill(newElement, method, definingAssembly, depth);

                foreach (var methodParameter in method.Parameters)
                {
                    var newParameter = new MyParameter(new MyType(methodParameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), depth + 1), methodParameter.Ordinal);
                    newElement.Depth = depth;
                    newElement.Parameters.Add(newParameter);
                    Fill(newParameter, methodParameter, definingAssembly, depth);
                }
            }
        }

        private static void Fill(MyProperty property, IPropertySymbol propertySymbol, IAssemblySymbol definingAssembly, int depth)
        {
            var propertyType = propertySymbol.Type;
            if (Equals(propertyType.ContainingAssembly, definingAssembly) && !propertyType.IsValueType)
            {
                // The parameter is a custom object
                // Extract properties and methods
                var classType = (INamedTypeSymbol)propertyType;
                Fill(property.Type, classType, definingAssembly, depth);
            }
        }

        private static void Fill(MyMethod method, IMethodSymbol methodSymbol, IAssemblySymbol definingAssembly, int depth)
        {
            var returnType = methodSymbol.ReturnType;
            if (Equals(returnType.ContainingAssembly, definingAssembly) && !returnType.IsValueType)
            {
                // The parameter is a custom object
                // Extract properties and methods
                var classType = (INamedTypeSymbol)returnType;
                Fill(method.ReturnType, classType, definingAssembly, depth);
            }
        }

        private static void Fill(MyParameter param, IParameterSymbol parameterSymbol, IAssemblySymbol definingAssembly, int depth)
        {
            var parameterType = parameterSymbol.Type;
            if (Equals(parameterType.ContainingAssembly.Name, definingAssembly.Name) && !parameterType.IsValueType)
            {
                // The parameter is a custom object
                // Extract properties and methods
                var classType = (INamedTypeSymbol)parameterType;
                Fill(param.Type, classType, definingAssembly, depth);
            }
        }
    }
}
