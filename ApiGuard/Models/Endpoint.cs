using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace ApiGuard.Models
{
    internal class Endpoint
    {
        public string MethodName { get; set; }
        public MyType ReturnType { get; set; }
        public List<INamedTypeSymbol> Attributes { get; set; } = new List<INamedTypeSymbol>();
        public List<MyParameter> Parameters { get; } = new List<MyParameter>();
    }
}
