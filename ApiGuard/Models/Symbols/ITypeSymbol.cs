using System.Collections.Generic;

namespace ApiGuard.Models.Symbols
{
    internal interface ITypeSymbol : ISymbol
    {
        TypeKind TypeKind { get; set; }
        List<string> Modifiers { get; set; }
    }
}
