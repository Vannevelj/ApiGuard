using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ApiGuard.Domain.Strategies.Interfaces;

[assembly: InternalsVisibleTo("ApiGuard.Tests")]
namespace ApiGuard.Models
{
    internal class Api : MyType
    {
        public Api(string typename) : base(typename, 0)
        {
        }

        public List<MyMethod> Endpoints => NestedElements.OfType<MyMethod>().ToList();

        public IEnumerable<EndpointResult> GetEndpointDifferences(Api otherApi, IEndpointMatchingStrategy endpointMatchingStrategy)
        {
            foreach (var endpoint in Endpoints)
            {
                var result = endpointMatchingStrategy.GetEndpoint(otherApi.Endpoints, endpoint);
                if (!result.IsExactMatch)
                {
                    yield return result;
                }
            }
        }
    }
}
