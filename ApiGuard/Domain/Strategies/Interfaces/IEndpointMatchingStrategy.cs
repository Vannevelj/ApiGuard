using System.Collections.Generic;
using ApiGuard.Models;

namespace ApiGuard.Domain.Strategies.Interfaces
{
    internal interface IEndpointMatchingStrategy
    {
        EndpointResult GetEndpoint(List<MyMethod> allEndpointsInNewApi, MyMethod existingEndpoint);

        bool TryGetChangedApiAttribute(Api oldApi, Api newApi, out MyAttribute attribute);
    }
}
