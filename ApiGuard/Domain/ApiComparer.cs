using System;
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

        public void Compare(MyType originalApi, MyType newApi)
        {
            var differences = _endpointMatchingStrategy.GetApiDifferences(originalApi, newApi);
            if (differences.Count == 0)
            {
                return;
            }

            var mismatch = differences.First();

            switch (mismatch.Reason)
            {
                case MismatchReason.ModifierChanged: 
                case MismatchReason.AttributeMismatch: ThrowAttributeMismatch(mismatch); break;
                case MismatchReason.ParameterNameChanged: ThrowParameterNameChanged(mismatch); break;
                case MismatchReason.TypeNameChanged: throw new DefinitionMismatchException(mismatch, withParentInfo: mismatch.Expected.Parent != null);
                case MismatchReason.ElementRemoved: throw new ElementRemovedException(mismatch);
                case MismatchReason.DefinitionChanged: throw new DefinitionMismatchException(mismatch);
                case MismatchReason.TypeKindChanged: throw new TypeKindChangedException(mismatch);
                case MismatchReason.MemberAddedToInterface: throw new MemberAddedToInterfaceException(mismatch);
                case MismatchReason.None:
                    throw new ArgumentException("Unspecified mismatch reason");
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
