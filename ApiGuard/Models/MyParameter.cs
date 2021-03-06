﻿using System;
using System.Collections.Generic;
using ApiGuard.Models.Symbols;

namespace ApiGuard.Models
{
    internal class MyParameter : IEquatable<MyParameter>, ISymbol
    {
        public MyType Type { get; set; }
        public int Ordinal { get; set; }
        public string Name { get; set; }
        public ISymbol Parent { get; set; }

        public MyParameter(string name, int ordinal)
        {
            Name = name;
            Ordinal = ordinal;
        }

        public override bool Equals(object obj) => Equals(obj as MyParameter);
        public bool Equals(MyParameter other)
        {
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return other != null &&
                   Ordinal == other.Ordinal &&
                   Type == other.Type &&
                   Name == other.Name;
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

        public string GetShorthandParent()
        {
            var method = (MyMethod) Parent;
            return $"{method.Parent.Name}.{method.Name}";
        }

        public override string ToString() => $"{Parent}";
    }
}
