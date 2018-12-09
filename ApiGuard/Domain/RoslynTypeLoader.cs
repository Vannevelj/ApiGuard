using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Exceptions;
using ApiGuard.Models;
using Microsoft.CodeAnalysis;
using ISymbol = ApiGuard.Models.Symbols.ISymbol;
using TypeKind = ApiGuard.Models.TypeKind;

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

            if (apiSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                throw new ApiNotPublicException(apiSymbol.Name);
            }

            var api = GetType(apiSymbol, definingAssembly, null);

            return api;
        }

        private static MyType GetType(ITypeSymbol typeSymbol, IAssemblySymbol definingAssembly, ISymbol parent)
        {
            var type = new MyType(GetName(typeSymbol))
            {
                Parent = parent
            };

            if (typeSymbol.TypeKind == Microsoft.CodeAnalysis.TypeKind.Class && !typeSymbol.IsAbstract)
            {
                type.TypeKind = TypeKind.Class;
            }
            else
            {
                if (typeSymbol.TypeKind == Microsoft.CodeAnalysis.TypeKind.Interface)
                {
                    type.TypeKind = TypeKind.Interface;
                }
                else if (typeSymbol.TypeKind == Microsoft.CodeAnalysis.TypeKind.Class)
                {
                    type.TypeKind = TypeKind.AbstractClass;
                }
            }

            if (Equals(typeSymbol.ContainingAssembly, definingAssembly) && !typeSymbol.IsValueType)
            {
                var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>().ToList();
                foreach (var propertySymbol in properties)
                {
                    var property = GetProperty(propertySymbol, definingAssembly, type);
                    type.NestedElements.Add(property);
                }

                var methods = typeSymbol.GetMembers().OfType<IMethodSymbol>().Where(x => x.CanBeReferencedByName && !x.ContainingNamespace.Name.StartsWith("System", StringComparison.InvariantCultureIgnoreCase)).ToList();
                foreach (var method in methods)
                {
                    var newElement = GetMethod(method, definingAssembly, type);
                    type.NestedElements.Add(newElement);
                }

                foreach (var attributeData in typeSymbol.GetAttributes())
                {
                    var attribute = GetAttribute(attributeData, type);
                    type.Attributes.Add(attribute);
                }

                type.Modifiers = GetModifiers(typeSymbol);
            }

            return type;
        }

        private static MyProperty GetProperty(IPropertySymbol propertySymbol, IAssemblySymbol definingAssembly, MyType parent)
        {
            var property = new MyProperty(propertySymbol.Name)
            {
                Parent = parent
            };

            property.Type = GetType(propertySymbol.Type, definingAssembly, property);
            property.Modifiers = GetModifiers(propertySymbol);

            foreach (var attributeData in propertySymbol.GetAttributes())
            {
                var attribute = GetAttribute(attributeData, property);
                property.Attributes.Add(attribute);
            }
            
            return property;
        }

        private static MyMethod GetMethod(IMethodSymbol methodSymbol, IAssemblySymbol definingAssembly, MyType parent)
        {
            var method = new MyMethod(methodSymbol.Name)
            {
                Parent = parent,
            };

            method.ReturnType = GetType(methodSymbol.ReturnType, definingAssembly, method);
            method.Modifiers = GetModifiers(methodSymbol);

            foreach (var parameterSymbol in methodSymbol.Parameters)
            {
                var parameter = GetParameter(parameterSymbol, definingAssembly, method);
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

        private static MyParameter GetParameter(IParameterSymbol parameterSymbol, IAssemblySymbol definingAssembly, ISymbol parent)
        {
            var parameter = new MyParameter(parameterSymbol.Name, parameterSymbol.Ordinal)
            {
                Parent = parent,
            };

            parameter.Type = GetType(parameterSymbol.Type, definingAssembly, parameter);

            return parameter;
        }

        private static string GetName(ITypeSymbol symbol) => symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

        private static List<string> GetModifiers(Microsoft.CodeAnalysis.ISymbol symbol)
        {
            var modifiers = new List<string>();

            if (symbol.IsStatic) { modifiers.Add("static"); }
            if (symbol.IsVirtual) { modifiers.Add("virtual"); }
            if (symbol.IsAbstract) { modifiers.Add("abstract"); }
            if (symbol.IsSealed) { modifiers.Add("sealed"); }

            if (symbol.DeclaredAccessibility == Accessibility.NotApplicable)
            {
                switch (symbol)
                {
                    case ITypeSymbol _: modifiers.Add("internal"); break;
                    case IMethodSymbol method:
                        var outerMethodType = method.ContainingType;
                        modifiers.Add(outerMethodType.TypeKind == Microsoft.CodeAnalysis.TypeKind.Class ? "private" : outerMethodType.DeclaredAccessibility.ToString().ToLowerInvariant());
                        break;
                    case IPropertySymbol property:
                        var outerPropertyType = property.ContainingType;
                        modifiers.Add(outerPropertyType.TypeKind == Microsoft.CodeAnalysis.TypeKind.Class ? "private" : outerPropertyType.DeclaredAccessibility.ToString().ToLowerInvariant());
                        break;
                }
            }
            else
            {
                modifiers.Add(symbol.DeclaredAccessibility.ToString().ToLowerInvariant());
            }

            return modifiers;
        }
    }
}
