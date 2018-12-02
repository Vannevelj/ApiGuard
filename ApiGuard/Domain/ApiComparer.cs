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
                // The API has no relevant endpoints
                if (!endpointResult.IsSameEndpoint)
                {
                    throw new EndpointNotFoundException(endpointResult.ExistingEndpoint);
                }

                var innerMostMismatch = endpointResult.SymbolsChanged.OrderByDescending(x => x.Received.Depth).First();
                if (innerMostMismatch.Received is MyAttribute newAttribute)
                {
                    throw new AttributeMismatchException(newAttribute);
                }

                throw new DefinitionMismatchException(innerMostMismatch);
            }
        }
    }
}
