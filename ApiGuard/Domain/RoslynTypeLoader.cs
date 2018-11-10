using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain.Interfaces;
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

            return api;
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
                    var classType = (INamedTypeSymbol)parameterType;
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
