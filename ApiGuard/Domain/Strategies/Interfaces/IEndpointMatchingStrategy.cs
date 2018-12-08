using System.Collections.Generic;
using ApiGuard.Models;

namespace ApiGuard.Domain.Strategies.Interfaces
{
    internal interface IEndpointMatchingStrategy
    {
        List<SymbolMismatch> GetApiDifferences(MyType originalApi, MyType newApi);
    }
}
