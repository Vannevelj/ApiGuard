using System.Collections.Generic;
using System.Linq;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Models;

namespace ApiGuard.Domain.Strategies
{
    internal class ExactEndpointMatchingStrategy : IEndpointMatchingStrategy
    {
        public EndpointResult GetEndpoint(List<MyMethod> allEndpointsInNewApi, MyMethod existingEndpoint)
        {
            var endpoint = allEndpointsInNewApi.SingleOrDefault(x => x == existingEndpoint);
            return new EndpointResult
            {
                ExistingEndpoint = endpoint,
                Differences = 0,
            };
        }
    }
}
