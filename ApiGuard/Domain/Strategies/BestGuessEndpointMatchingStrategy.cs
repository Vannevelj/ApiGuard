using System;
using System.Collections.Generic;
using System.Linq;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Models;

namespace ApiGuard.Domain.Strategies
{
    internal class BestGuessEndpointMatchingStrategy : IEndpointMatchingStrategy
    {
        public EndpointResult GetEndpoint(IEnumerable<MyMethod> allEndpointsInNewApi, MyMethod existingEndpoint)
        {
            var endPointResults = new List<EndpointResult>();

            foreach (var newEndpoint in allEndpointsInNewApi)
            {
                var symbolsChanged = new List<SymbolMismatch>();
                Compare(existingEndpoint, newEndpoint, symbolsChanged);

                endPointResults.Add(new EndpointResult
                {
                    ExistingEndpoint = existingEndpoint,
                    ReceivedEndpoint = newEndpoint,
                    SymbolsChanged = symbolsChanged
                });
            }

            if (!endPointResults.Any())
            {
                return new EndpointResult
                {
                    ExistingEndpoint = existingEndpoint,
                    ReceivedEndpoint = null,
                    SymbolsChanged = new List<SymbolMismatch>
                    {
                        new SymbolMismatch(MismatchReason.ElementRemoved, existingEndpoint, null)
                    }
                };
            }

            var minimalDifference = endPointResults.OrderBy(x => x.SymbolsChanged.Count).First();
            return minimalDifference;
        }

        public bool TryGetChangedApiAttribute(MyType oldApi, MyType newApi, out MyAttribute attribute)
        {
            var symbols = new List<SymbolMismatch>();
            Compare(oldApi.Attributes, newApi.Attributes, symbols);

            if (symbols.Any())
            {
                var symbolMismatch = symbols.First();
                attribute = symbolMismatch.Expected as MyAttribute ?? symbolMismatch.Received as MyAttribute;
                return attribute != null;
            }

            attribute = null;
            return false;
        }

        private void AddMismatch(MismatchReason reason, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            symbols.Add(new SymbolMismatch(reason, expectedSymbol, newSymbol));
        }

        private bool Compare<T>(MismatchReason reason, T existingValue, T newValue, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (!EqualityComparer<T>.Default.Equals(existingValue, newValue))
            {
                AddMismatch(reason, symbols, expectedSymbol, newSymbol);
                return false;
            }

            return true;
        }

        private void Compare(MyType existingType, MyType newType, List<SymbolMismatch> symbols)
        {
            Compare(MismatchReason.TypeChanged, existingType.Name, newType.Name, symbols, existingType, newType);
            Compare(existingType.NestedElements, newType.NestedElements, symbols);
            Compare(existingType.Attributes, newType.Attributes, symbols);
        }

        private void Compare(List<ISymbol> existingSymbols, List<ISymbol> newSymbols, List<SymbolMismatch> symbols)
        {
            var removedSymbols = existingSymbols.Where(e => !newSymbols.Select(n => n.Name).Contains(e.Name)).ToList();

            if (removedSymbols.Any())
            {
                removedSymbols.ForEach(x => AddMismatch(MismatchReason.ElementRemoved, symbols, x, null));
                return;
            }

            for (var i = 0; i < existingSymbols.Count; i++)
            {
                var element1 = existingSymbols[i];
                var element2 = newSymbols[i]; // TODO: better algorithm that searches for the symbol so we are no longer dependent on the same ordering

                switch (element1)
                {
                    case MyProperty firstProperty:
                        Compare(firstProperty, (MyProperty) element2, symbols);
                        break;
                    case MyMethod firstMethod:
                        Compare(firstMethod, (MyMethod) element2, symbols);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported element: {element1.GetType()}");
                }
            }
        }

        private void Compare(MyProperty existingProperty, MyProperty newProperty, List<SymbolMismatch> symbols)
        {
            Compare(MismatchReason.ElementRemoved, existingProperty.Name, newProperty.Name, symbols, existingProperty, newProperty);
            Compare(existingProperty.Attributes, newProperty.Attributes, symbols);
            Compare(existingProperty.Type, newProperty.Type, symbols);
        }

        private void Compare(MyMethod existingMethod, MyMethod newMethod, List<SymbolMismatch> symbols)
        {
            Compare(MismatchReason.ElementRemoved, existingMethod.Name, newMethod.Name, symbols, existingMethod, newMethod);
            Compare(existingMethod.ReturnType, newMethod.ReturnType, symbols);
            Compare(existingMethod.Parameters, newMethod.Parameters, symbols, existingMethod, newMethod);
            Compare(existingMethod.Attributes, newMethod.Attributes, symbols);
        }

        private void Compare(List<MyParameter> existingParameters, List<MyParameter> newParameters, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (!Compare(MismatchReason.DefinitionChanged, existingParameters.Count, newParameters.Count, symbols, expectedSymbol, newSymbol))
            {
                return;
            }

            // We rely on ordering since that is part of the API for methods
            for (var i = 0; i < existingParameters.Count; i++)
            {
                var param1 = existingParameters[i];
                var param2 = newParameters[i];
                Compare(param1, param2, symbols);
            }
        }

        private void Compare(MyParameter existingParameter, MyParameter newParameter, List<SymbolMismatch> symbols)
        {
            Compare(MismatchReason.DefinitionChanged, existingParameter.Ordinal, newParameter.Ordinal, symbols, existingParameter, newParameter);
            Compare(existingParameter.Type, newParameter.Type, symbols);
            Compare(MismatchReason.ParameterNameChanged, existingParameter.Name, newParameter.Name, symbols, existingParameter, newParameter);
        }

        private void Compare(List<MyAttribute> existingAttributes, List<MyAttribute> newAttributes, List<SymbolMismatch> symbols)
        {
            var addedAttributes = newAttributes.Where(n => !existingAttributes.Select(e => e.Name).Contains(n.Name)).ToList();
            var removedAttributes = existingAttributes.Where(e => !newAttributes.Select(n => n.Name).Contains(e.Name)).ToList();

            if (removedAttributes.Any())
            {
                removedAttributes.ForEach(x => AddMismatch(MismatchReason.ElementRemoved, symbols, x, null));
                return;
            }

            if (addedAttributes.Any())
            {
                addedAttributes.ForEach(x => AddMismatch(MismatchReason.AttributeMismatch, symbols, null, x));
                return;
            }

            for (var i = 0; i < existingAttributes.Count; i++)
            {
                var attr1 = existingAttributes[i];
                var attr2 = newAttributes.First(x => x.Name == attr1.Name);
                Compare(attr1, attr2, symbols);
            }
        }

        private void Compare(MyAttribute existingAttribute, MyAttribute newAttribute, List<SymbolMismatch> symbols)
        {
            Compare(MismatchReason.AttributeMismatch, existingAttribute.Name, newAttribute.Name, symbols, existingAttribute, newAttribute);

            if (newAttribute.Values.Count != existingAttribute.Values.Count)
            {
                AddMismatch(MismatchReason.AttributeMismatch, symbols, existingAttribute, newAttribute);
                return;
            }

            foreach (var value in existingAttribute.Values)
            {
                var correspondingAttribute = newAttribute.Values.FirstOrDefault(x => x.Key == value.Key);
                Compare(MismatchReason.AttributeMismatch, value.Value, correspondingAttribute.Value, symbols, existingAttribute, newAttribute);
            }
        }
    }
}
