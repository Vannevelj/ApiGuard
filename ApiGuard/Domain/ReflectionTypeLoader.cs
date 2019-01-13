using ApiGuard.Domain.Interfaces;
using ApiGuard.Exceptions;
using ApiGuard.Models;
using ApiGuard.Models.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ApiGuard.Domain
{
    internal class ReflectionTypeLoader : ITypeLoader
    {
        private static readonly Dictionary<Type, string> Aliases =
                    new Dictionary<Type, string>()
                {
                    { typeof(byte), "byte" },
                    { typeof(sbyte), "sbyte" },
                    { typeof(short), "short" },
                    { typeof(ushort), "ushort" },
                    { typeof(int), "int" },
                    { typeof(uint), "uint" },
                    { typeof(long), "long" },
                    { typeof(ulong), "ulong" },
                    { typeof(float), "float" },
                    { typeof(double), "double" },
                    { typeof(decimal), "decimal" },
                    { typeof(object), "object" },
                    { typeof(bool), "bool" },
                    { typeof(char), "char" },
                    { typeof(string), "string" },
                    { typeof(void), "void" }
                };
         
        public Task<MyType> LoadApi(object input)
        {
            var apiSymbol = (Type) input;
            
            if (!(apiSymbol.IsPublic || apiSymbol.IsNestedPublic))
            {
                throw new ApiNotPublicException(apiSymbol.Name);
            }

            var api = GetType(apiSymbol, apiSymbol.Assembly, null);

            return Task.FromResult(api);
        }

        private MyType GetType(Type typeSymbol, Assembly definingAssembly, ISymbol parent)
        {
            var type = new MyType(GetName(typeSymbol))
            {
                Parent = parent
            };

            if (typeSymbol.IsClass && !typeSymbol.IsAbstract)
            {
                type.TypeKind = TypeKind.Class;
            }
            else
            {
                if (typeSymbol.IsInterface)
                {
                    type.TypeKind = TypeKind.Interface;
                }
                else if (typeSymbol.IsClass)
                {
                    type.TypeKind = TypeKind.AbstractClass;
                }
            }

            if (Equals(typeSymbol.Assembly, definingAssembly) && !typeSymbol.IsValueType)
            {
                var properties = typeSymbol.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var propertySymbol in properties)
                {
                    var property = GetProperty(propertySymbol, definingAssembly, type);
                    type.NestedElements.Add(property);
                }

                var methods = typeSymbol.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance)
                                        .Where(x =>
                                            !x.DeclaringType.Namespace.StartsWith("System", StringComparison.InvariantCultureIgnoreCase) && // Excludes base methods on Object
                                            !x.IsSpecialName); // Excludes generated methods like get_Name and set_Name
                foreach (var method in methods)
                {
                    var newElement = GetMethod(method, definingAssembly, type);
                    type.NestedElements.Add(newElement);
                }

                foreach (var attributeData in typeSymbol.GetCustomAttributesData())
                {
                    var attribute = GetAttribute(attributeData, type);
                    type.Attributes.Add(attribute);
                }

                type.Modifiers = GetModifiers(typeSymbol);
            }

            return type;
        }

        private string GetName(Type typeSymbol) => Aliases.TryGetValue(typeSymbol, out var name) ? name : typeSymbol.Name;

        private MyProperty GetProperty(PropertyInfo propertySymbol, Assembly definingAssembly, ISymbol parent)
        {
            var property = new MyProperty(propertySymbol.Name)
            {
                Parent = parent
            };

            property.Type = GetType(propertySymbol.PropertyType, definingAssembly, property);
            property.Modifiers = GetModifiers(propertySymbol);

            foreach (var attributeData in propertySymbol.GetCustomAttributesData())
            {
                var attribute = GetAttribute(attributeData, property);
                property.Attributes.Add(attribute);
            }

            return property;
        }

        private MyMethod GetMethod(MethodInfo methodSymbol, Assembly definingAssembly, ISymbol parent)
        {
            var method = new MyMethod(methodSymbol.Name)
            {
                Parent = parent,
            };

            method.ReturnType = GetType(methodSymbol.ReturnType, definingAssembly, method);
            method.Modifiers = GetModifiers(methodSymbol);

            foreach (var parameterSymbol in methodSymbol.GetParameters())
            {
                var parameter = GetParameter(parameterSymbol, definingAssembly, method);
                method.Parameters.Add(parameter);
            }

            foreach (var attributeData in methodSymbol.GetCustomAttributesData())
            {
                var attribute = GetAttribute(attributeData, method);
                method.Attributes.Add(attribute);
            }

            return method;
        }

        private MyAttribute GetAttribute(CustomAttributeData attributeData, ISymbol parent)
        {
            var values = new Dictionary<string, string>();
            foreach (var namedArgument in attributeData.NamedArguments)
            {
                values.Add(namedArgument.MemberName, namedArgument.TypedValue.Value.ToString());
            }

            var attribute = new MyAttribute(attributeData.AttributeType.Name, values)
            {
                Parent = parent
            };

            return attribute;
        }

        private MyParameter GetParameter(ParameterInfo parameterSymbol, Assembly definingAssembly, ISymbol parent)
        {
            var parameter = new MyParameter(parameterSymbol.Name, parameterSymbol.Position)
            {
                Parent = parent,
            };

            parameter.Type = GetType(parameterSymbol.ParameterType, definingAssembly, parameter);

            return parameter;
        }

        private List<string> GetModifiers(PropertyInfo property)
        {
            // https://stackoverflow.com/a/20807747/1864167
            return GetModifiers(property.GetGetMethod()).Union(GetModifiers(property.GetSetMethod())).ToList();
        }

        private List<string> GetModifiers(MethodInfo method)
        {
            var modifiers = new List<string>();
            
            if (method.IsStatic) { modifiers.Add("static"); }
            if (method.IsVirtual) { modifiers.Add("virtual"); }
            if (method.IsAbstract) { modifiers.Add("abstract"); }

            string modifier = null;
            if (method.IsPrivate)
            {
                modifier = "private";
            }

            if (method.IsFamily)
            {
                modifier = "protected";
            }

            if (method.IsAssembly)
            {
                modifier = "internal";
            }

            if (method.IsPublic)
            {
                modifier = "public";
            }

            if (modifier == null)
            {
                if (method.DeclaringType.IsClass)
                {
                    modifier = "private";
                }
                else
                {
                    modifier = "public";
                }
            }

            modifiers.Add(modifier);
            
            return modifiers;
        }

        private List<string> GetModifiers(Type type)
        {
            var modifiers = new List<string>();

            if (type.IsAbstract) { modifiers.Add("abstract"); }
            if (type.IsSealed) { modifiers.Add("sealed"); }

            string modifier = null;
            if (type.IsNestedPrivate)
            {
                modifier = "private";
            }

            if (type.IsNestedFamily)
            {
                modifier = "protected";
            }

            if (type.IsNotPublic)
            {
                modifier = "internal";
            }

            if (type.IsPublic)
            {
                modifier = "public";
            }

            if (modifier == null)
            {
                if (type.DeclaringType.IsClass)
                {
                    modifier = "private";
                }
                else
                {
                    modifier = "public";
                }
            }

            modifiers.Add(modifier);

            return modifiers;
        }
    }
}
