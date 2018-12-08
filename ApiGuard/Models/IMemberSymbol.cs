using System.Collections.Generic;

namespace ApiGuard.Models
{
    internal interface IMemberSymbol : ISymbol
    {
        List<string> Modifiers { get; set; }
    }
}