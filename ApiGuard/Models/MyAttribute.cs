using System;
using System.Collections.Generic;
using ApiGuard.Models.Symbols;

namespace ApiGuard.Models
{
    internal class MyAttribute : IEquatable<MyAttribute>, ISymbol
    {
        public string Name { get; set;  }
        public Dictionary<string, string> Values { get; }

        public ISymbol Parent { get; set; }

        public MyAttribute(string name, Dictionary<string, string> values)
        {
            Name = name;
            Values = values;
        }

        public override bool Equals(object obj) => Equals(obj as MyAttribute);
        public bool Equals(MyAttribute other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            var hashCode = -707644969;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public static bool operator ==(MyAttribute attribute1, MyAttribute attribute2) => EqualityComparer<MyAttribute>.Default.Equals(attribute1, attribute2);
        public static bool operator !=(MyAttribute attribute1, MyAttribute attribute2) => !(attribute1 == attribute2);

        public override string ToString() => Name;
    }
}
