using System;
using System.Collections.Generic;

namespace ApiGuard.Models
{
    internal class MyParameter : IEquatable<MyParameter>
    {
        public MyType Type { get; }
        public int Ordinal { get; }

        public MyParameter(MyType type, int ordinal)
        {
            Type = type;
            Ordinal = ordinal;
        }

        public override bool Equals(object obj) => Equals(obj as MyParameter);
        public bool Equals(MyParameter other)
        {
            return other != null &&
                   Ordinal == other.Ordinal &&
                   Type == other.Type;
        }

        public override int GetHashCode()
        {
            var hashCode = -707644969;
            hashCode = hashCode * -1521134295 + EqualityComparer<MyType>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + Ordinal.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(MyParameter parameter1, MyParameter parameter2) => EqualityComparer<MyParameter>.Default.Equals(parameter1, parameter2);
        public static bool operator !=(MyParameter parameter1, MyParameter parameter2) => !(parameter1 == parameter2);
    }
}
