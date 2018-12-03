using System.Linq;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Exceptions;
using ApiGuard.Models;

namespace ApiGuard.Domain
{
    internal class ApiComparer : IApiComparer
    {
        private readonly IEndpointMatchingStrategy _endpointMatchingStrategy;

        public ApiComparer(IEndpointMatchingStrategy endpointMatchingStrategy)
        {
            _endpointMatchingStrategy = endpointMatchingStrategy;
        }

        public void Compare(Api originalApi, Api newApi)
        {
            if (originalApi.Name != newApi.Name)
            {
                throw new ApiNotFoundException(originalApi.Name);
            }

            if (_endpointMatchingStrategy.TryGetChangedApiAttribute(originalApi, newApi, out var apiAttribute))
            {
                throw new AttributeMismatchException(apiAttribute);
            }

            foreach (var endpointResult in originalApi.GetEndpointDifferences(newApi, _endpointMatchingStrategy))
            {
                // The API has no relevant endpoints
                if (!endpointResult.IsSameEndpoint)
                {
                    throw new EndpointNotFoundException(endpointResult.ExistingEndpoint);
                }

                var innerMostMismatch = endpointResult.SymbolsChanged.OrderByDescending(x => x.Received?.Depth ?? x.Expected?.Depth).First();
                if (innerMostMismatch.Received is MyAttribute || innerMostMismatch.Expected is MyAttribute)
                {
                    var attribute = (MyAttribute) (innerMostMismatch.Received ?? innerMostMismatch.Expected);
                    throw new AttributeMismatchException(attribute);
                }

                if (innerMostMismatch.Received == null)
                {
                    throw new ElementRemovedException(innerMostMismatch);
                }

                throw new DefinitionMismatchException(innerMostMismatch);
            }
        }
    }
}
