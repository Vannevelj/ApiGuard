using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ApiGuard.Models.Symbols;

[assembly: InternalsVisibleTo("ApiGuard.Tests")]
namespace ApiGuard.Models
{
    internal class MyType : IEquatable<MyType>, ITypeSymbol
    {
        public string Name { get; set; }
        public ISymbol Parent { get; set; }
        public List<IMemberSymbol> NestedElements { get; set; } = new List<IMemberSymbol>();
        public List<MyAttribute> Attributes { get; set; } = new List<MyAttribute>();
        public TypeKind TypeKind { get; set; }
        public List<MyType> GenericTypeArguments { get; set; } = new List<MyType>();

        public List<string> Modifiers { get; set; } = new List<string>();

        public MyType(string typename)
        {
            Name = typename;
        }

        public bool Equals(MyType other)
        {
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return other != null &&
                   Name == other.Name &&
                   Attributes.SequenceEqual(other.Attributes) &&
                   GenericTypeArguments.SequenceEqual(other.GenericTypeArguments) &&
                   TypeKind == other.TypeKind &&
                   NestedElements.SequenceEqual(other.NestedElements);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MyType);
        }

        public override int GetHashCode()
        {
            var hashCode = 1892093377;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<IMemberSymbol>>.Default.GetHashCode(NestedElements);
            return hashCode;
        }

        public static bool operator ==(MyType type1, MyType type2) => EqualityComparer<MyType>.Default.Equals(type1, type2);
        public static bool operator !=(MyType type1, MyType type2) => !(type1 == type2);

        public override string ToString()
        {
            var name = Name;

            if (!GenericTypeArguments.Any())
            {
                return name;
            }
            
            var tokens = new List<string>();
            foreach (var arg in GenericTypeArguments)
            {
                tokens.Add(arg.ToString());
            }

            if (name.StartsWith("Nullable", StringComparison.InvariantCultureIgnoreCase))
            {
                return $"{tokens[0]}?";
            }

            return $"{name}<{string.Join(", ", tokens)}>";
        }
    }
}