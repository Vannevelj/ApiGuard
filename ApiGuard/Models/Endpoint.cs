using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiGuard.Models
{
    internal class Endpoint : IEquatable<Endpoint>
    {
        public string MethodName { get; set; }
        public MyType ReturnType { get; set; }
        public List<MyType> Attributes { get; set; } = new List<MyType>();
        public List<MyParameter> Parameters { get; } = new List<MyParameter>();

        public override bool Equals(object obj) => Equals(obj as Endpoint);

        public bool Equals(Endpoint other)
        {
            if (other == null ||
                MethodName != other.MethodName ||
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
            var hashCode = 2087104799;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MethodName);
            hashCode = hashCode * -1521134295 + EqualityComparer<MyType>.Default.GetHashCode(ReturnType);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<MyType>>.Default.GetHashCode(Attributes);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<MyParameter>>.Default.GetHashCode(Parameters);
            return hashCode;
        }

        public static bool operator ==(Endpoint endpoint1, Endpoint endpoint2) => EqualityComparer<Endpoint>.Default.Equals(endpoint1, endpoint2);
        public static bool operator !=(Endpoint endpoint1, Endpoint endpoint2) => !(endpoint1 == endpoint2);

        public override string ToString()
        {
            return $"{ReturnType.Typename} {MethodName}({string.Join(", ", Parameters.OrderBy(x => x.Ordinal).Select(x => x.Type.Typename))})";
        }
    }
}
