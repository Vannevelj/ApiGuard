using System.Collections.Generic;
using System.Linq;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Models;

namespace ApiGuard.Domain.Strategies
{
    internal class ExactEndpointMatchingStrategy : IEndpointMatchingStrategy
    {
        public EndpointResult GetEndpoint(List<MyMethod> existingEndpoints, MyMethod otherEndpoint)
        {
            var endpoint = existingEndpoints.SingleOrDefault(x => x == otherEndpoint);
            return new EndpointResult
            {
                ExistingEndpoint = endpoint,
                Differences = 0,
            };
        }
    }
}
