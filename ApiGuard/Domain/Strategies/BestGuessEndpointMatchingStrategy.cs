using System;
using System.Collections.Generic;
using System.Linq;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Models;
using ApiGuard.Models.Symbols;

namespace ApiGuard.Domain.Strategies
{
    internal class BestGuessEndpointMatchingStrategy : IEndpointMatchingStrategy
    {
        public List<SymbolMismatch> GetApiDifferences(MyType originalApi, MyType newApi)
        {
            var symbols = new List<SymbolMismatch>();
            Compare(originalApi, newApi, symbols);

            return symbols;
        }

        private void AddMismatch(List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol, MismatchReason reason)
        {
            symbols.Add(new SymbolMismatch(reason, expectedSymbol, newSymbol));
        }

        private bool Compare<T>(T existingValue, T newValue, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol, MismatchReason reason)
        {
            if (!EqualityComparer<T>.Default.Equals(existingValue, newValue))
            {
                AddMismatch(symbols, expectedSymbol, newSymbol, reason);
                return false;
            }

            return true;
        }

        private void Compare(MyType existingType, MyType newType, List<SymbolMismatch> symbols)
        {
            Compare(existingType.Name, newType.Name, symbols, existingType, newType, MismatchReason.TypeNameChanged);
            Compare(existingType.NestedElements, newType.NestedElements, symbols);
            Compare(existingType.Attributes, newType.Attributes, symbols);
            Compare(existingType.Modifiers, newType.Modifiers, symbols, existingType, newType);
            Compare(existingType.TypeKind, newType.TypeKind, symbols, existingType, newType, MismatchReason.TypeKindChanged);

            if (newType.NestedElements.Count > existingType.NestedElements.Count && (newType.TypeKind == TypeKind.AbstractClass || newType.TypeKind == TypeKind.Interface))
            {
                AddMismatch(symbols, existingType, newType, MismatchReason.MemberAddedToInterface);
            }

            CompareGenericArguments(existingType, newType, symbols);
        }

        private void CompareGenericArguments(MyType existingType, MyType newType, List<SymbolMismatch> symbols)
        {
            if (existingType.GenericTypeArguments.Count != newType.GenericTypeArguments.Count)
            {
                AddMismatch(symbols, existingType, newType, MismatchReason.DefinitionChanged);
                return;
            }

            for (var i = 0; i < existingType.GenericTypeArguments.Count; i++)
            {
                var existingTypeArgument = existingType.GenericTypeArguments[i];
                var correspondingType = newType.GenericTypeArguments[i];
                Compare(existingTypeArgument, correspondingType, symbols);
            }
        }

        private void Compare(List<IMemberSymbol> existingSymbols, List<IMemberSymbol> newSymbols, List<SymbolMismatch> symbols)
        {
            foreach (var existingSymbol in existingSymbols)
            {
                if (!IsPartOfTheApi(existingSymbol.Modifiers))
                {
                    continue;
                }

                var correspondingNewSymbol = GetCorrespondingSymbol(existingSymbol, newSymbols);
                if (correspondingNewSymbol == null)
                {
                    AddMismatch(symbols, existingSymbol, null, MismatchReason.ElementRemoved);
                    continue;
                }

                switch (existingSymbol)
                {
                    case MyProperty firstProperty:
                        Compare(firstProperty, (MyProperty)correspondingNewSymbol, symbols);
                        break;
                    case MyMethod firstMethod:
                        Compare(firstMethod, (MyMethod)correspondingNewSymbol, symbols);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported element: {existingSymbol.GetType()}");
                }
            }
        }

        private ISymbol GetCorrespondingSymbol(IMemberSymbol current, List<IMemberSymbol> allSymbols)
        {
            var potentialCandidates = allSymbols.Where(x => x.Name == current.Name && x.GetType() == current.GetType()).ToList();

            if (current is MyProperty)
            {
                return potentialCandidates.FirstOrDefault();
            }

            if (current is MyMethod currentMethod)
            {
                if (potentialCandidates.Count <= 1)
                {
                    return potentialCandidates.FirstOrDefault();
                }
                else
                {
                    var potentialMethods = potentialCandidates.OfType<MyMethod>();
                    var newSymbolWithSameParameters = potentialMethods.FirstOrDefault(x => x.Parameters.SequenceEqual(currentMethod.Parameters));
                    if (newSymbolWithSameParameters != null)
                    {
                        return newSymbolWithSameParameters;
                    }

                    return null;
                }
            }

            throw new ArgumentException($"Unsupported symbol: {current.GetType()}");
        }

        private void Compare(MyProperty existingProperty, MyProperty newProperty, List<SymbolMismatch> symbols)
        {
            Compare(existingProperty.Name, newProperty.Name, symbols, existingProperty, newProperty, MismatchReason.ElementRemoved);
            Compare(existingProperty.Attributes, newProperty.Attributes, symbols);
            Compare(existingProperty.Type, newProperty.Type, symbols);
            Compare(existingProperty.Modifiers, newProperty.Modifiers, symbols, existingProperty, newProperty);
        }

        private void Compare(MyMethod existingMethod, MyMethod newMethod, List<SymbolMismatch> symbols)
        {
            Compare(existingMethod.Name, newMethod.Name, symbols, existingMethod, newMethod, MismatchReason.ElementRemoved);
            Compare(existingMethod.ReturnType, newMethod.ReturnType, symbols);
            Compare(existingMethod.Parameters, newMethod.Parameters, symbols, existingMethod, newMethod);
            Compare(existingMethod.Attributes, newMethod.Attributes, symbols);
            Compare(existingMethod.Modifiers, newMethod.Modifiers, symbols, existingMethod, newMethod);
        }
        
        private void Compare(List<MyParameter> existingParameters, List<MyParameter> newParameters, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (!Compare(existingParameters.Count, newParameters.Count, symbols, expectedSymbol, newSymbol, MismatchReason.DefinitionChanged))
            {
                return;
            }

            // We rely on ordering since that is part of the signature for methods
            for (var i = 0; i < existingParameters.Count; i++)
            {
                var param1 = existingParameters[i];
                var param2 = newParameters[i];
                Compare(param1, param2, symbols);
            }
        }

        private void Compare(MyParameter existingParameter, MyParameter newParameter, List<SymbolMismatch> symbols)
        {
            Compare(existingParameter.Ordinal, newParameter.Ordinal, symbols, existingParameter, newParameter, MismatchReason.DefinitionChanged);
            Compare(existingParameter.Type, newParameter.Type, symbols);
            Compare(existingParameter.Name, newParameter.Name, symbols, existingParameter, newParameter, MismatchReason.ParameterNameChanged);
        }

        private void Compare(List<MyAttribute> existingAttributes, List<MyAttribute> newAttributes, List<SymbolMismatch> symbols)
        {
            var addedAttributes = newAttributes.Where(n => !existingAttributes.Select(e => e.Name).Contains(n.Name)).ToList();
            var removedAttributes = existingAttributes.Where(e => !newAttributes.Select(n => n.Name).Contains(e.Name)).ToList();

            if (removedAttributes.Any())
            {
                removedAttributes.ForEach(x => AddMismatch(symbols, x, null, MismatchReason.ElementRemoved));
                return;
            }

            if (addedAttributes.Any())
            {
                addedAttributes.ForEach(x => AddMismatch(symbols, null, x, MismatchReason.AttributeMismatch));
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
            Compare(existingAttribute.Name, newAttribute.Name, symbols, existingAttribute, newAttribute, MismatchReason.AttributeMismatch);

            if (newAttribute.Values.Count != existingAttribute.Values.Count)
            {
                AddMismatch(symbols, existingAttribute, newAttribute, MismatchReason.AttributeMismatch);
                return;
            }

            foreach (var value in existingAttribute.Values)
            {
                var correspondingAttribute = newAttribute.Values.FirstOrDefault(x => x.Key == value.Key);
                Compare(value.Value, correspondingAttribute.Value, symbols, existingAttribute, newAttribute, MismatchReason.AttributeMismatch);
            }
        }

        private void Compare(List<string> existingModifiers, List<string> newModifiers, List<SymbolMismatch> symbols, ISymbol expectedSymbol, ISymbol newSymbol)
        {
            if (!Compare(existingModifiers.Count, newModifiers.Count, symbols, expectedSymbol, newSymbol, MismatchReason.ModifierChanged))
            {
                return;
            }

            foreach (var existingModifier in existingModifiers)
            {
                if (!newModifiers.Contains(existingModifier))
                {
                    AddMismatch(symbols, expectedSymbol, newSymbol, MismatchReason.ModifierChanged);
                }
            }
        }

        private bool IsPartOfTheApi(List<string> modifiers) => modifiers.Any(x => x == "public" || x == "protected");
    }
}
