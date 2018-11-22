using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace ApiGuard.Domain.Strategies.Interfaces
{
    internal interface IRoslynSymbolProvider
    {
        Task<INamedTypeSymbol> GetApiClassSymbol(object input);
    }
}
