using System.Collections.Generic;
using System.Linq;
using ApiGuard.Domain.Strategies.Interfaces;

namespace ApiGuard.Models
{
    internal class Api
    {
        private readonly IEndpointMatchingStrategy _endpointMatchingStrategy;

        public Api(IEndpointMatchingStrategy endpointMatchingStrategy)
        {
            _endpointMatchingStrategy = endpointMatchingStrategy;
        }

        public string TypeName { get; set; }
        public List<Endpoint> Endpoints { get; set; } = new List<Endpoint>();

        public EndpointResult GetMatchingEndpoint(Endpoint otherEndpoint)
        {
            return _endpointMatchingStrategy.GetEndpoint(Endpoints, otherEndpoint);
        }
    }
}
