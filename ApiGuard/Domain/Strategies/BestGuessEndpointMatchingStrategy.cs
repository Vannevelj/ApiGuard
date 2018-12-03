using System;
using System.Collections.Generic;
using System.Linq;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Models;

namespace ApiGuard.Domain.Strategies
{
    internal class BestGuessEndpointMatchingStrategy : IEndpointMatchingStrategy
    {
        public EndpointResult GetEndpoint(List<MyMethod> allEndpointsInNewApi, MyMethod existingEndpoint)
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
                        new SymbolMismatch(existingEndpoint, null)
                    }
                };
            }

            var minimalDifference = endPointResults.OrderBy(x => x.SymbolsChanged.Count).First();
            return minimalDifference;
        }

        public bool TryGetChangedApiAttribute(Api oldApi, Api newApi, out MyAttribute attribute)
        {
            var symbols = new List<SymbolMismatch>();
            Compare(oldApi.Attributes, newApi.Attributes, symbols, oldApi, newApi);

            if (symbols.Any())
            {
                var symbolMismatch = symbols.First();
                attribute = symbolMismatch.Expected as MyAttribute ?? symbolMismatch.Received as MyAttribute;
                return attribute != null;
            }

            attribute = null;
            return false;
        }

        private void AddMismatch(List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            symbols.Add(new SymbolMismatch(expectedSymbol, newSymbol));
        }

        private bool Compare<T>(T existingValue, T newValue, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (!EqualityComparer<T>.Default.Equals(existingValue, newValue))
            {
                AddMismatch(symbols, expectedSymbol, newSymbol);
                return false;
            }

            return true;
        }

        private void Compare(List<MyType> existingTypes, List<MyType> newTypes, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (existingTypes.Count < newTypes.Count)
            {
                AddMismatch(symbols, expectedSymbol, newSymbol);
                return;
            }

            for (var i = 0; i < existingTypes.Count; i++)
            {
                var type1 = existingTypes[i];
                var type2 = newTypes[i];
                Compare(type1, type2, symbols, expectedSymbol, expectedSymbol);
            }
        }

        private void Compare(MyType existingType, MyType newType, List<SymbolMismatch> symbols)
        {
            Compare(existingType.Name, newType.Name, symbols, existingType, newType);
            Compare(existingType.NestedElements, newType.NestedElements, symbols, existingType, newType);
            Compare(existingType.Attributes, newType.Attributes, symbols, existingType, newType);
        }

        private void Compare(List<ISymbol> existingSymbols, List<ISymbol> newSymbols, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            var removedSymbols = existingSymbols.Where(e => !newSymbols.Select(n => n.Name).Contains(e.Name)).ToList();

            if (removedSymbols.Any())
            {
                removedSymbols.ForEach(x => AddMismatch(symbols, x, null));
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
                    case MyParameter firstParameter:
                        Compare(firstParameter, (MyParameter) element2, symbols, element1.Parent, element2.Parent);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported element: {element1.GetType()}");
                }
            }
        }

        private void Compare(MyProperty existingProperty, MyProperty newProperty, List<SymbolMismatch> symbols)
        {
            Compare(existingProperty.Name, newProperty.Name, symbols, existingProperty, newProperty);
            Compare(existingProperty.Attributes, newProperty.Attributes, symbols, existingProperty, newProperty);

            if (existingProperty.Type.NestedElements.Any())
            {
                Compare(existingProperty.Type, newProperty.Type, symbols);
            }
            else
            {
                Compare(existingProperty.Type, newProperty.Type, symbols, existingProperty, newProperty);
            }
        }

        private void Compare(MyMethod existingMethod, MyMethod newMethod, List<SymbolMismatch> symbols)
        {
            Compare(existingMethod.Name, newMethod.Name, symbols, existingMethod, newMethod);
            Compare(existingMethod.ReturnType, newMethod.ReturnType, symbols);
            Compare(existingMethod.Parameters, newMethod.Parameters, symbols, existingMethod, newMethod);
            Compare(existingMethod.Attributes, newMethod.Attributes, symbols, existingMethod, newMethod);
        }

        private void Compare(List<MyParameter> existingParameters, List<MyParameter> newParameters, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (!Compare(existingParameters.Count, newParameters.Count, symbols, expectedSymbol, newSymbol))
            {
                return;
            }

            // We rely on ordering since that is part of the API for methods
            for (var i = 0; i < existingParameters.Count; i++)
            {
                var param1 = existingParameters[i];
                var param2 = newParameters[i];
                Compare(param1, param2, symbols, expectedSymbol, newSymbol);
            }
        }

        private void Compare(MyParameter existingParameter, MyParameter newParameter, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            Compare(existingParameter.Ordinal, newParameter.Ordinal, symbols, expectedSymbol, newSymbol);
            Compare(existingParameter.Type, newParameter.Type, symbols);
        }

        private void Compare(List<MyAttribute> existingAttributes, List<MyAttribute> newAttributes, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            var addedAttributes = newAttributes.Where(n => !existingAttributes.Select(e => e.Name).Contains(n.Name)).ToList();
            var removedAttributes = existingAttributes.Where(e => !newAttributes.Select(n => n.Name).Contains(e.Name)).ToList();

            if (removedAttributes.Any())
            {
                removedAttributes.ForEach(x => AddMismatch(symbols, x, null));
                return;
            }

            if (addedAttributes.Any())
            {
                addedAttributes.ForEach(x => AddMismatch(symbols, null, x));
                return;
            }

            for (var i = 0; i < existingAttributes.Count; i++)
            {
                var attr1 = existingAttributes[i];
                var attr2 = newAttributes.First(x => x.Name == attr1.Name);
                Compare(attr1, attr2, symbols, attr1, attr2);
            }
        }

        private void Compare(MyAttribute existingAttribute, MyAttribute newAttribute, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            Compare(existingAttribute.Name, newAttribute.Name, symbols, expectedSymbol, newSymbol);

            if (newAttribute.Values.Count != existingAttribute.Values.Count)
            {
                AddMismatch(symbols, expectedSymbol, newSymbol);
                return;
            }

            foreach (var value in existingAttribute.Values)
            {
                var correspondingAttribute = newAttribute.Values.FirstOrDefault(x => x.Key == value.Key);
                Compare(value.Value, correspondingAttribute.Value, symbols, expectedSymbol, newSymbol);
            }
        }
    }
}
