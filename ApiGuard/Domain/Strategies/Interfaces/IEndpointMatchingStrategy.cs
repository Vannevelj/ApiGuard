using System.Collections.Generic;
using ApiGuard.Models;

namespace ApiGuard.Domain.Strategies.Interfaces
{
    internal interface IEndpointMatchingStrategy
    {
        EndpointResult GetEndpoint(IEnumerable<MyMethod> allEndpointsInNewApi, MyMethod existingEndpoint);

        bool TryGetChangedApiAttribute(MyType oldApi, MyType newApi, out MyAttribute attribute);
    }
}
