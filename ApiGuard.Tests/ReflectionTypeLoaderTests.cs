using ApiGuard.Domain;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Domain.Strategies;
using ApiGuard.Exceptions;
using ApiGuard.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace ApiGuard.Tests
{
    public partial class ReflectionTypeLoaderTests
    {
        internal static List<SymbolMismatch> GetApiDifferences(MyType originalApi, MyType newApi)
        {
            var strategy = new BestGuessEndpointMatchingStrategy();
            return strategy.GetApiDifferences(originalApi, newApi);
        }

        internal static async Task<MyType> GetApi(Type type)
        {
            var typeLoader = new ReflectionTypeLoader();
            return await typeLoader.LoadApi(type);
        }

        internal static async Task Compare(Type originalApi, Type newApi)
        {
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            new Domain.ApiComparer(new BestGuessEndpointMatchingStrategy())
                .Compare(firstApi, secondApi);
        }
    }
}
