using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.Domain.Strategies;
using ApiGuard.Models;
using Xunit;

namespace ApiGuard.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task DifferentEndpointReturnType()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
";

            var newApi = @"
public class MyApi
{
    public bool FirstMethod() { return true; }
}
";

            var symbolProvider = new SourceCodeRoslynSymbolProvider();
            var typeLoader = new RoslynTypeLoader(symbolProvider);
            var firstApi = await typeLoader.LoadApi(originalApi);
            var secondApi = await typeLoader.LoadApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task IdenticalTopLevelEndpoints()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod() { return 99; }
}
";

            var symbolProvider = new SourceCodeRoslynSymbolProvider();
            var typeLoader = new RoslynTypeLoader(symbolProvider);
            var firstApi = await typeLoader.LoadApi(originalApi);
            var secondApi = await typeLoader.LoadApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Empty(differences);
        }
    }
}
