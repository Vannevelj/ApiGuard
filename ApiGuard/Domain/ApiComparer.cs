using System.Linq;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Exceptions;
using ApiGuard.Models;

namespace ApiGuard.Domain
{
    internal class ApiComparer : IApiComparer
    {
        public void Compare(Api originalApi, Api newApi)
        {
            if (originalApi.TypeName != newApi.TypeName)
            {
                throw new ApiNotFoundException(originalApi.TypeName);
            }

            foreach (var endpointResult in originalApi.GetApiDifferences(newApi))
            {
                var differentEndpointDefinition = endpointResult.SymbolsChanged.SingleOrDefault(x => x.Received.Equals(endpointResult.ExistingEndpoint));
                if (differentEndpointDefinition != null)
                {
                    throw new EndpointNotFoundException(endpointResult.ExistingEndpoint, newApi.TypeName);
                }

                var innerMostMismatch = endpointResult.SymbolsChanged.OrderByDescending(x => x.Received.Depth).First();
                throw new DefinitionMismatchException(innerMostMismatch);
            }
        }
    }
}
