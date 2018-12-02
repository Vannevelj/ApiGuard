using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Domain.Strategies;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Models;
using Microsoft.CodeAnalysis;
using ISymbol = Microsoft.CodeAnalysis.ISymbol;

namespace ApiGuard.Domain
{
    internal class RoslynTypeLoader : ITypeLoader
    {
        private readonly IRoslynSymbolProvider _roslynSymbolProvider;

        internal RoslynTypeLoader(IRoslynSymbolProvider roslynSymbolProvider) => _roslynSymbolProvider = roslynSymbolProvider;

        public async Task<Api> LoadApi(object input)
        {
            var apiSymbol = await _roslynSymbolProvider.GetApiClassSymbol(input);
            var definingAssembly = apiSymbol.ContainingAssembly;

            var topSymbol = new MyType(GetName(apiSymbol), 0);
            
            // For each method create an entry (Endpoint)
            // Each Endpoint contains the name of the method, return type and its arguments as well as targeted attributes
            // If the Type is a complex object, we repeat the process for that object but also include properties
            var endpoints = new List<MyMethod>();
            var depth = 1;
            foreach (var methodSymbol in apiSymbol.GetMembers().OfType<IMethodSymbol>().Where(x => x.CanBeReferencedByName))
            {
                var endpoint = new MyMethod(
                    methodSymbol.Name,
                    new MyType(GetName(methodSymbol.ReturnType), depth + 1))
                {
                    Attributes = methodSymbol.GetAttributes().Select(x => x.AttributeClass).Select(x => new MyType(GetName(x), depth)).ToList(),
                    Depth = depth,
                    Parent = topSymbol
                };

                foreach (var parameter in methodSymbol.Parameters)
                {
                    var param =
                        new MyParameter(new MyType(GetName(parameter.Type), depth + 1), parameter.Ordinal)
                        {
                            Depth = depth
                        };
                    Fill(param, parameter, definingAssembly, depth);
                    endpoint.Parameters.Add(param);
                }

                Fill(endpoint.ReturnType, methodSymbol.ReturnType, definingAssembly, depth);

                endpoints.Add(endpoint);
            }

            var api = new Api(new BestGuessEndpointMatchingStrategy())
            {
                TypeName = GetName(apiSymbol),
                Endpoints = endpoints
            };

            return api;
        }
        
        private static void Fill(MyType type, INamespaceOrTypeSymbol complexObject, IAssemblySymbol definingAssembly, int depth)
        {
            depth++;
            var properties = complexObject.GetMembers().OfType<IPropertySymbol>().ToList();
            foreach (var property in properties)
            {
                var newElement = new MyProperty
                {
                    Name = property.Name,
                    Type = new MyType(GetName(property.Type), depth + 1),
                    Depth = depth,
                    Parent = type
                };

                type.NestedElements.Add(newElement);
                Fill(newElement, property, definingAssembly, depth);
            }

            var methods = complexObject.GetMembers().OfType<IMethodSymbol>().Where(x => x.CanBeReferencedByName && !x.ContainingNamespace.Name.StartsWith("System", StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (var method in methods)
            {
                var newElement = new MyMethod(method.Name, new MyType(GetName(method.ReturnType), depth + 1))
                {
                    Depth = depth,
                    Parent = type
                };
                type.NestedElements.Add(newElement);
                Fill(newElement, method, definingAssembly, depth);

                foreach (var methodParameter in method.Parameters)
                {
                    var newParameter = new MyParameter(new MyType(GetName(methodParameter.Type), depth + 1), methodParameter.Ordinal);
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

        private static string GetName(ITypeSymbol symbol) => symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
    }
}
