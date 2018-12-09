using System.Collections.Generic;

namespace ApiGuard.Models.Symbols
{
    internal interface IMemberSymbol : ISymbol
    {
        List<string> Modifiers { get; set; }
    }
}