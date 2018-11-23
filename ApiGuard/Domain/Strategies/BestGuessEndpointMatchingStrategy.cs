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
                var differences = 0;
                var symbolsChanged = new List<SymbolMismatch>();
                Compare(existingEndpoint, newEndpoint, ref differences, symbolsChanged, existingEndpoint, newEndpoint);

                endPointResults.Add(new EndpointResult
                {
                    ExistingEndpoint = existingEndpoint,
                    ReceivedEndpoint = newEndpoint,
                    Differences = differences,
                    SymbolsChanged = symbolsChanged
                });
            }

            var minimalDifference = endPointResults.OrderBy(x => x.Differences).First();
            return minimalDifference;
        }

        private void AddMismatch(List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol, ref int counter)
        {
            counter++;
            symbols.Add(new SymbolMismatch(expectedSymbol, newSymbol));
        }

        private bool Compare<T>(T existingValue, T newValue, ref int counter, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (!EqualityComparer<T>.Default.Equals(existingValue, newValue))
            {
                AddMismatch(symbols, expectedSymbol, newSymbol, ref counter);
                return false;
            }

            return true;
        }

        private void Compare(List<MyType> existingTypes, List<MyType> newTypes, ref int counter, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (existingTypes.Count < newTypes.Count)
            {
                AddMismatch(symbols, expectedSymbol, newSymbol, ref counter);
                return;
            }

            for (var i = 0; i < existingTypes.Count; i++)
            {
                var type1 = existingTypes[i];
                var type2 = newTypes[i];
                Compare(type1, type2, ref counter, symbols, expectedSymbol, expectedSymbol);
            }
        }

        private void Compare(MyType existingType, MyType newType, ref int counter, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            Compare(existingType.Typename, newType.Typename, ref counter, symbols, existingType, newType);
            Compare(existingType.NestedElements, newType.NestedElements, ref counter, symbols, existingType, newType);
        }

        private void Compare(List<ISymbol> existingSymbols, List<ISymbol> newSymbols, ref int counter, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (newSymbols.Count < existingSymbols.Count)
            {
                AddMismatch(symbols, expectedSymbol, newSymbol, ref counter);
                return;
            }

            for (var i = 0; i < existingSymbols.Count; i++)
            {
                var element1 = existingSymbols[i];
                var element2 = newSymbols[i]; // TODO: better algorithm that searches for the symbol so we are no longer dependent on the same ordering
                Compare(element1, element2, ref counter, symbols, expectedSymbol, newSymbol);
            }
        }

        private void Compare(MyProperty existingProperty, MyProperty newProperty, ref int counter, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            Compare(existingProperty.Name, newProperty.Name, ref counter, symbols, expectedSymbol, newSymbol);
            Compare(existingProperty.Type, newProperty.Type, ref counter, symbols, expectedSymbol, newSymbol);
        }

        private void Compare(MyMethod existingMethod, MyMethod newMethod, ref int counter, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            Compare(existingMethod.Name, newMethod.Name, ref counter, symbols, existingMethod, newMethod);
            Compare(existingMethod.ReturnType, newMethod.ReturnType, ref counter, symbols, existingMethod, newMethod);
            Compare(existingMethod.Parameters, newMethod.Parameters, ref counter, symbols, existingMethod, newMethod);
            Compare(existingMethod.Attributes, newMethod.Attributes, ref counter, symbols, existingMethod, newMethod);
        }

        private void Compare(List<MyParameter> existingParameters, List<MyParameter> newParameters, ref int counter, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (!Compare(existingParameters.Count, newParameters.Count, ref counter, symbols, expectedSymbol, newSymbol))
            {
                return;
            }

            for (var i = 0; i < existingParameters.Count; i++)
            {
                var param1 = existingParameters[i];
                var param2 = newParameters[i];
                Compare(param1, param2, ref counter, symbols, expectedSymbol, newSymbol);
            }
        }

        private void Compare(MyParameter existingParameter, MyParameter newParameter, ref int counter, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            Compare(existingParameter.Ordinal, newParameter.Ordinal, ref counter, symbols, expectedSymbol, newSymbol);
            Compare(existingParameter.Type, newParameter.Type, ref counter, symbols, expectedSymbol, newSymbol);
        }
    }
}
