using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiGuard.Models
{
    internal class MyMethod : IElement, IEquatable<MyMethod>
    {
        public MyType ReturnType { get; }
        public string Name { get; }
        public List<MyParameter> Parameters { get; } = new List<MyParameter>();

        public MyMethod(string name, MyType returnType)
        {
            Name = name;
            ReturnType = returnType;
        }

        public override bool Equals(object obj) => Equals(obj as MyMethod);
        public bool Equals(MyMethod other)
        {
            return other != null &&
                   Name == other.Name &&
                   ReturnType == other.ReturnType &&
                   Parameters.SequenceEqual(other.Parameters);
        }

        public override int GetHashCode()
        {
            var hashCode = -1177465050;
            hashCode = hashCode * -1521134295 + EqualityComparer<MyType>.Default.GetHashCode(ReturnType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<MyParameter>>.Default.GetHashCode(Parameters);
            return hashCode;
        }

        public static bool operator ==(MyMethod method1, MyMethod method2) => EqualityComparer<MyMethod>.Default.Equals(method1, method2);
        public static bool operator !=(MyMethod method1, MyMethod method2) => !(method1 == method2);
    }
}