using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApiGuard.Tests")]
namespace ApiGuard.Models
{
    internal class MyType : IEquatable<MyType>, ISymbol
    {
        public int Depth { get; set; }
        public string Name { get; set; }
        public ISymbol Parent { get; set; }
        public List<ISymbol> NestedElements { get; set; } = new List<ISymbol>();
        public List<MyAttribute> Attributes { get; set; } = new List<MyAttribute>();
        public List<string> Modifiers { get; set; } = new List<string>();

        public MyType(string typename, int depth)
        {
            Name = typename;
            Depth = depth;
        }

        public bool Equals(MyType other)
        {
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return other != null &&
                   Name == other.Name &&
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
            hashCode = hashCode * -1521134295 + EqualityComparer<List<ISymbol>>.Default.GetHashCode(NestedElements);
            return hashCode;
        }

        public static bool operator ==(MyType type1, MyType type2) => EqualityComparer<MyType>.Default.Equals(type1, type2);
        public static bool operator !=(MyType type1, MyType type2) => !(type1 == type2);

        public override string ToString() => $"{Name}";
    }
}