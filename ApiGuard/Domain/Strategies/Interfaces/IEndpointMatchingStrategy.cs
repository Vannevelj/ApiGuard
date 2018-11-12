using System.Collections.Generic;
using ApiGuard.Models;

namespace ApiGuard.Domain.Strategies.Interfaces
{
    internal interface IEndpointMatchingStrategy
    {
        EndpointResult GetEndpoint(List<MyMethod> existingEndpoints, MyMethod otherEndpoint);
    }
}
