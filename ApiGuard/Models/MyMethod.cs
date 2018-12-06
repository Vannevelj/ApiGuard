using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiGuard.Models
{
    internal class MyMethod : IEquatable<MyMethod>, ISymbol
    {
        public MyType ReturnType { get; set; }
        public string Name { get; set; }
        public int Depth { get; set; }
        public ISymbol Parent { get; set; }
        public List<MyAttribute> Attributes { get; set; } = new List<MyAttribute>();
        public List<MyParameter> Parameters { get; set; } = new List<MyParameter>();

        public MyMethod(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj) => Equals(obj as MyMethod);
        public bool Equals(MyMethod other)
        {
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            if (other == null ||
                Name != other.Name ||
                Parameters.Count != other.Parameters.Count ||
                ReturnType != other.ReturnType)
            {
                return false;
            }

            if (other.Parameters.Count == 0)
            {
                return true;
            }

            for (var index = 0; index < Parameters.Count; index++)
            {
                var param = Parameters[index];
                var matchingParam = other.Parameters[index];

                if (param == matchingParam)
                {
                    return true;
                }

                return false;
            }

            return false;
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

        public override string ToString()
            => $"{ReturnType.Name} {Parent}.{Name}({string.Join(", ", Parameters.OrderBy(x => x.Ordinal).Select(x => x.Type.Name))})";
    }
}