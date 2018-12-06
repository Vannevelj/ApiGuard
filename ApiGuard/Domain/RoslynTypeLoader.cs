using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain.Interfaces;
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

        public async Task<MyType> LoadApi(object input)
        {
            var apiSymbol = await _roslynSymbolProvider.GetApiClassSymbol(input);
            var definingAssembly = apiSymbol.ContainingAssembly;

            var api = GetType(apiSymbol, definingAssembly, null, 0);

            return api;
        }

        private static MyType GetType(ITypeSymbol complexObject, IAssemblySymbol definingAssembly, ISymbol parent, int depth)
        {
            var type = new MyType(GetName(complexObject), depth)
            {
                Parent = parent
            };
            depth++;

            if (Equals(complexObject.ContainingAssembly, definingAssembly) && !complexObject.IsValueType)
            {
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

                foreach (var attributeData in complexObject.GetAttributes())
                {
                    var attribute = GetAttribute(attributeData, type);
                    type.Attributes.Add(attribute);
                }
            }

            return type;
        }

        private static MyProperty GetProperty(IPropertySymbol propertySymbol, IAssemblySymbol definingAssembly, MyType parent, int depth)
        {
            var property = new MyProperty(propertySymbol.Name)
            {
                Parent = parent
            };

            property.Type = GetType(propertySymbol.Type, definingAssembly, property, depth + 1);

            foreach (var attributeData in propertySymbol.GetAttributes())
            {
                var attribute = GetAttribute(attributeData, property);
                property.Attributes.Add(attribute);
            }

            return property;
        }

        private static MyMethod GetMethod(IMethodSymbol methodSymbol, IAssemblySymbol definingAssembly, MyType parent, int depth)
        {
            var method = new MyMethod(methodSymbol.Name)
            {
                Parent = parent,
            };

            method.ReturnType = GetType(methodSymbol.ReturnType, definingAssembly, method, depth);

            foreach (var parameterSymbol in methodSymbol.Parameters)
            {
                var parameter = GetParameter(parameterSymbol, definingAssembly, method, depth);
                method.Parameters.Add(parameter);
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
            var parameter = new MyParameter(parameterSymbol.Name, parameterSymbol.Ordinal)
            {
                Parent = parent,
                Depth = depth,
            };

            parameter.Type = GetType(parameterSymbol.Type, definingAssembly, parameter, depth);

            return parameter;
        }

        private static string GetName(ITypeSymbol symbol) => symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
    }
}
