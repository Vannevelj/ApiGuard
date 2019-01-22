using ApiGuard.Domain;
using ApiGuard.Domain.Strategies;
using ApiGuard.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiGuard.Tests
{
    public partial class ReflectionTypeLoaderTests
    {
        internal static List<SymbolMismatch> GetApiDifferences(MyType originalApi, MyType newApi)
        {
            var strategy = new BestGuessEndpointMatchingStrategy();
            return strategy.GetApiDifferences(originalApi, newApi);
        }

        internal static MyType GetApi(Type type)
        {
            var typeLoader = new ReflectionTypeLoader();
            return typeLoader.LoadApi(type);
        }

        internal static void Compare(Type originalApi, Type newApi)
        {
            var firstApi = GetApi(originalApi);
            var secondApi = GetApi(newApi);

            new ApiComparer(new BestGuessEndpointMatchingStrategy())
                .Compare(firstApi, secondApi);
        }
    }
}
