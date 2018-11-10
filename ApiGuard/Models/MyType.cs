using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiGuard.Models
{
    internal class MyType : IEquatable<MyType>
    {
        public string Typename { get; }
        public List<IElement> NestedElements { get; } = new List<IElement>();

        public MyType(string typename)
        {
            Typename = typename;
        }

        public bool Equals(MyType other)
        {
            return other != null &&
                Typename == other.Typename &&
                NestedElements.SequenceEqual(other.NestedElements);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MyType);
        }

        public override int GetHashCode()
        {
            var hashCode = 1892093377;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Typename);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<IElement>>.Default.GetHashCode(NestedElements);
            return hashCode;
        }

        public static bool operator ==(MyType type1, MyType type2) => EqualityComparer<MyType>.Default.Equals(type1, type2);
        public static bool operator !=(MyType type1, MyType type2) => !(type1 == type2);
    }
}