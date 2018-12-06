using System.Collections.Generic;
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

        public IEnumerable<EndpointResult> GetEndpointDifferences(MyType oldApi, MyType newApi)
        {
            foreach (var endpoint in oldApi.NestedElements.OfType<MyMethod>())
            {
                var result = _endpointMatchingStrategy.GetEndpoint(newApi.NestedElements.OfType<MyMethod>(), endpoint);
                if (!result.IsExactMatch)
                {
                    yield return result;
                }
            }
        }

        public void Compare(MyType originalApi, MyType newApi)
        {
            if (originalApi.Name != newApi.Name)
            {
                throw new ApiNotFoundException(originalApi.Name);
            }

            if (_endpointMatchingStrategy.TryGetChangedApiAttribute(originalApi, newApi, out var apiAttribute))
            {
                throw new AttributeMismatchException(apiAttribute);
            }

            foreach (var endpointResult in GetEndpointDifferences(originalApi, newApi))
            {
                // The API has no relevant endpoints
                if (!endpointResult.IsSameEndpoint)
                {
                    throw new EndpointNotFoundException(endpointResult.ExistingEndpoint);
                }

                var innerMostMismatch = endpointResult.SymbolsChanged.OrderByDescending(x => x.Received?.Depth ?? x.Expected?.Depth).First();

                switch (innerMostMismatch.Reason)
                {
                    case MismatchReason.AttributeMismatch: ThrowAttributeMismatch(innerMostMismatch); break;
                    case MismatchReason.ParameterNameChanged: ThrowParameterNameChanged(innerMostMismatch); break;
                    case MismatchReason.TypeChanged: throw new DefinitionMismatchException(innerMostMismatch, withParentInfo: true);
                    case MismatchReason.ElementRemoved: throw new ElementRemovedException(innerMostMismatch);
                    case MismatchReason.DefinitionChanged: throw new DefinitionMismatchException(innerMostMismatch);
                }
            }
        }

        private void ThrowAttributeMismatch(SymbolMismatch mismatch)
        {
            var attribute = (MyAttribute)(mismatch.Received ?? mismatch.Expected);
            throw new AttributeMismatchException(attribute);
        }

        private void ThrowParameterNameChanged(SymbolMismatch mismatch)
        {
            var oldParameter = (MyParameter) mismatch.Expected;
            var newParameter = (MyParameter) mismatch.Received;
            throw new ParameterNameMismatchException(oldParameter, newParameter);
        }
    }
}
