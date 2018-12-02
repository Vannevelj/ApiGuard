using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Domain.Strategies;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Models;
using Microsoft.CodeAnalysis;
using ISymbol = ApiGuard.Models.ISymbol;

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
                var endpoint = GetMethod(methodSymbol, definingAssembly, topSymbol, depth);
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
            foreach (var propertySymbol in properties)
            {
                var property = GetProperty(propertySymbol, definingAssembly, type, depth);
                type.NestedElements.Add(property);
            }

            var methods = complexObject.GetMembers().OfType<IMethodSymbol>().Where(x => x.CanBeReferencedByName && !x.ContainingNamespace.Name.StartsWith("System", StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (var method in methods)
            {
                var newElement = GetMethod(method, definingAssembly, type, depth);
                type.NestedElements.Add(newElement);
            }
        }

        private static MyProperty GetProperty(IPropertySymbol propertySymbol, IAssemblySymbol definingAssembly, MyType parent, int depth)
        {
            var property = new MyProperty(propertySymbol.Name, new MyType(GetName(propertySymbol.Type), depth + 1))
            {
                Parent = parent
            };

            var propertyType = propertySymbol.Type;
            if (Equals(propertyType.ContainingAssembly, definingAssembly) && !propertyType.IsValueType)
            {
                // The parameter is a custom object
                // Extract properties and methods
                var classType = (INamedTypeSymbol)propertyType;
                Fill(property.Type, classType, definingAssembly, depth);
            }

            foreach (var attributeData in propertySymbol.GetAttributes())
            {
                var attribute = GetAttribute(attributeData, property);
                property.Attributes.Add(attribute);
            }

            return property;
        }

        private static MyMethod GetMethod(IMethodSymbol methodSymbol, IAssemblySymbol definingAssembly, MyType parent, int depth)
        {
            var method = new MyMethod(methodSymbol.Name, new MyType(GetName(methodSymbol.ReturnType), depth + 1))
            {
                Parent = parent
            };

            foreach (var parameterSymbol in methodSymbol.Parameters)
            {
                var parameter = GetParameter(parameterSymbol, definingAssembly, method, depth);
                method.Parameters.Add(parameter);
            }

            var returnType = methodSymbol.ReturnType;
            if (Equals(returnType.ContainingAssembly, definingAssembly) && !returnType.IsValueType)
            {
                // The parameter is a custom object
                // Extract properties and methods
                var classType = (INamedTypeSymbol)returnType;
                Fill(method.ReturnType, classType, definingAssembly, depth);
            }

            foreach (var attributeData in methodSymbol.GetAttributes())
            {
                var attribute = GetAttribute(attributeData, method);
                method.Attributes.Add(attribute);
            }

            return method;
        }

        private static MyAttribute GetAttribute(AttributeData attributeData, ISymbol parent)
        {
            var values = new Dictionary<string, string>();
            foreach (var namedArgument in attributeData.NamedArguments)
            {
                values.Add(namedArgument.Key, namedArgument.Value.Value.ToString());
            }

            var attribute = new MyAttribute(GetName(attributeData.AttributeClass), values)
            {
                Parent = parent
            };

            return attribute;
        }

        private static MyParameter GetParameter(IParameterSymbol parameterSymbol, IAssemblySymbol definingAssembly, ISymbol parent, int depth)
        {
            var parameter = new MyParameter(new MyType(GetName(parameterSymbol.Type), depth + 1), parameterSymbol.Ordinal)
            {
                Parent = parent,
                Depth = depth,
                Name = parameterSymbol.Name,
            };

            var parameterType = parameterSymbol.Type;
            if (Equals(parameterType.ContainingAssembly?.Name, definingAssembly.Name) && !parameterType.IsValueType)
            {
                // The parameter is a custom object
                // Extract properties and methods
                var classType = (INamedTypeSymbol)parameterType;
                Fill(parameter.Type, classType, definingAssembly, depth);
            }

            return parameter;
        }

        private static string GetName(ITypeSymbol symbol) => symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
    }
}
