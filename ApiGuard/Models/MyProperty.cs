using System;
using System.Collections.Generic;
using System.Linq;
using ApiGuard.Models.Symbols;

namespace ApiGuard.Models
{
    internal class MyProperty : IEquatable<MyProperty>, IMemberSymbol
    {
        public MyType Type { get; set; }
        public string Name { get; set; }
        public ISymbol Parent { get; set; }
        public List<MyAttribute> Attributes { get; set; } = new List<MyAttribute>();
        public List<string> Modifiers { get; set; } = new List<string>();

        public MyProperty(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj) => Equals(obj as MyProperty);
        public bool Equals(MyProperty other)
        {
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return other != null &&
                   Name == other.Name &&
                   Type == other.Type &&
                   Attributes.SequenceEqual(other.Attributes) &&
                   Modifiers.SequenceEqual(other.Modifiers);
        }

        public override int GetHashCode()
        {
            var hashCode = -1979447941;
            hashCode = hashCode * -1521134295 + EqualityComparer<MyType>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public static bool operator ==(MyProperty property1, MyProperty property2) => EqualityComparer<MyProperty>.Default.Equals(property1, property2);
        public static bool operator !=(MyProperty property1, MyProperty property2) => !(property1 == property2);

        public override string ToString() => $"{Parent.Name}.{Name} ({Type})";
    }
}