using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.Domain.Strategies;
using ApiGuard.Models;
using Xunit;

namespace ApiGuard.Tests
{
    public class ApiTests
    {
        private async Task<Api> GetApi(string source)
        {
            var symbolProvider = new SourceCodeRoslynSymbolProvider();
            var typeLoader = new RoslynTypeLoader(symbolProvider);
            return await typeLoader.LoadApi(source);
        }

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
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

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

            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Empty(differences);
        }

        [Fact]
        public async Task DifferentName()
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
    public int SecondMethod() { return 32; }
}
";

            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task AdditionalEndpoint()
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
    public int FirstMethod() { return 32; }
    public bool SecondMethod() { return true; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Empty(differences);
        }

        [Fact]
        public async Task AdditionalEndpoint_WithChange()
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
    public double FirstMethod() { return 32.0; }
    public bool SecondMethod() { return true; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task RemovedEndpoint_WithChange()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod() { return 32; }
    public bool SecondMethod() { return true; }
}
";

            var newApi = @"
public class MyApi
{
    public double FirstMethod() { return 32.0; } 
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Equal(2, differences.Count);
        }

        [Fact]
        public async Task AdditionalParameter()
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
    public int FirstMethod(int x) { return 32; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task DifferentParameterType()
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
    public int FirstMethod(bool x) { return 32; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType()
        {
            var originalApi = @"
public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}

public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}
";

            var newApi = @"
public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}

public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ComplexParameterType_PropertyNameChanged()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string NewValue { get; set; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_PropertyTypeChanged()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public object Value { get; set; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_PropertyAdded()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
    public int Id { get; set; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ComplexParameterType_PropertyRemoved()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MethodNameChanged()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomethingElse() { }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MethodReturnTypeChanged()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public Task DoSomethingElse() { return Task.CompletedTask; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MethodAdded()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
    public void DoSomethingElse() { }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MethodRemoved()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{

}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MultipleChanges()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public int Key { get; set; }
    public string DoSomething() { return null; }
}
";

            var newApi = @"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething(object o) { }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexReturnType_MethodRemoved()
        {
            var originalApi = @"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething() { }
}
";

            var newApi = @"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{

}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexReturnType_MethodAdded()
        {
            var originalApi = @"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething() { }
}
";

            var newApi = @"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething() { }
    public string DoSomethingElse() { return null; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ComplexReturnType_MethodChanged()
        {
            var originalApi = @"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething() { }
}
";

            var newApi = @"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething(int o) { }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Single(differences);
        }

        [Fact]
        public async Task EndPointRemoved_AndEndpointChanged()
        {
            var originalApi = @"
public class MyApi
{
    public Opts FirstMethod() { return null; }
    public void SecondMethod() { }
}

public class Opts
{
    public void DoSomething() { }
}
";

            var newApi = @"
public class MyApi
{
    public NewOptions FirstMethod() { return null; }
}

public class NewOptions
{
    public void DoSomething() { }
    public string DoSomethingElse() { return null; }
}
";
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = firstApi.GetApiDifferences(secondApi).ToList();

            Assert.Equal(2, differences.Count);
        }
    }
}
