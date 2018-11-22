using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ApiGuard.Domain.Strategies.Interfaces;

[assembly: InternalsVisibleTo("ApiGuard.Tests")]
namespace ApiGuard.Models
{
    internal class Api
    {
        private readonly IEndpointMatchingStrategy _endpointMatchingStrategy;

        public Api(IEndpointMatchingStrategy endpointMatchingStrategy) => _endpointMatchingStrategy = endpointMatchingStrategy;

        public string TypeName { get; set; }
        public List<MyMethod> Endpoints { get; set; } = new List<MyMethod>();

        public IEnumerable<EndpointResult> GetApiDifferences(Api otherApi)
        {
            foreach (var endpoint in otherApi.Endpoints)
            {
                var result = _endpointMatchingStrategy.GetEndpoint(Endpoints, endpoint);
                if (!result.IsExactMatch)
                {
                    yield return result;
                }
            }
        }
    }
}
