using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.Domain.Strategies;
using ApiGuard.Models;
using Xunit;

namespace ApiGuard.Tests
{
    public class ApiTests : BaseTest
    {
        private async Task<MyType> GetApi(string source)
        {
            var symbolProvider = new SourceCodeRoslynSymbolProvider();
            var typeLoader = new RoslynTypeLoader(symbolProvider);
            return await typeLoader.LoadApi(source);
        }

        [Fact]
        public async Task DifferentEndpointReturnType()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public bool FirstMethod() { return true; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task IdenticalTopLevelEndpoints()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 99; }
}
");

            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task DifferentName()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int SecondMethod() { return 32; }
}
");

            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task AdditionalEndpoint()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 32; }
    public bool SecondMethod() { return true; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task AdditionalEndpoint_WithChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public double FirstMethod() { return 32.0; }
    public bool SecondMethod() { return true; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task RemovedEndpoint_WithChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 32; }
    public bool SecondMethod() { return true; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public double FirstMethod() { return 32.0; } 
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Equal(2, differences.Count);
        }

        [Fact]
        public async Task AdditionalParameter()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(int x) { return 32; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task DifferentParameterType()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(bool x) { return 32; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task DifferentParameterName()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(int abc) { return 32; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(int def) { return 32; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType()
        {
            var originalApi = GetApiFile(@"
public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}

public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}
");

            var newApi = GetApiFile(@"
public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}

public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ComplexParameterType_PropertyNameChanged()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string NewValue { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_PropertyTypeChanged()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public object Value { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_PropertyAdded()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}
");

            var newApi = GetApiFile(@"
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
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ComplexParameterType_PropertyRemoved()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
    public string Value { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public string Key { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MethodNameChanged()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomethingElse() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MethodReturnTypeChanged()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public Task DoSomethingElse() { return Task.CompletedTask; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MethodAdded()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
    public void DoSomethingElse() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MethodRemoved()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{

}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexParameterType_MultipleChanges()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public int Key { get; set; }
    public string DoSomething() { return null; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int FirstMethod(Opts o) { return 32; }
}

public class Opts
{
    public void DoSomething(object o) { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexReturnType_MethodRemoved()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{

}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ComplexReturnType_MethodAdded()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething() { }
    public string DoSomethingElse() { return null; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ComplexReturnType_MethodChanged()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public Opts FirstMethod() { return null; }
}

public class Opts
{
    public void DoSomething(int o) { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task EndPointRemoved_AndEndpointChanged()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public Opts FirstMethod() { return null; }
    public void SecondMethod() { }
}

public class Opts
{
    public void DoSomething() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public NewOptions FirstMethod() { return null; }
}

public class NewOptions
{
    public void DoSomething() { }
    public string DoSomethingElse() { return null; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Equal(2, differences.Count);
        }

        [Fact]
        public async Task AttributeValueChanged()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [DataMember(Order = 1)]
    public int Data { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [DataMember(Order = 2)]
    public int Data { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task AttributeRemoved()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [DataMember(Order = 1)]
    public int Data { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    public int Data { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task AttributeRemoved_OnMethod()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    [Obsolete]
    public void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task AttributeAdded()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    public int Data { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [DataMember(Order = 1)]
    public int Data { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task AttributeAdded_OnApiType()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
[Obsolete]
public class MyApi
{
    public void FirstMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var matchingStrategy = new BestGuessEndpointMatchingStrategy();
            var hasDifference = matchingStrategy.TryGetChangedApiAttribute(firstApi, secondApi, out var attribute);

            Assert.True(hasDifference);
            Assert.Equal("ObsoleteAttribute", attribute.Name);
        }

        [Fact]
        public async Task AttributeValueAdded()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [DataMember(Order = 1)]
    public int Data { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [DataMember(Order = 1, Name = ""MyData"")]
    public int Data { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task SwappedMethod_NoChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
    public void SecondMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void SecondMethod() { }
    public void FirstMethod() { } 
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task SwappedMethod_WithChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
    public void SecondMethod(int a) { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void SecondMethod(string a) { }
    public void FirstMethod() { } 
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task SwappedProperty_NoChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public int FirstProperty { get; set; }
    public int SecondProperty { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int SecondProperty { get; set; }
    public int FirstProperty { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task SwappedProperty_WithChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public double FirstProperty { get; set; }
    public int SecondProperty { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public int SecondProperty { get; set; }
    public int FirstProperty { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task SwappedAttribute_NoChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [DataMember(Order = 1)]
    [Obsolete]
    public int Data { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [Obsolete]
    [DataMember(Order = 1)]
    public int Data { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task SwappedAttribute_WithChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [DataMember(Order = 1)]
    [Obsolete]
    public int Data { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod(Args a) { }
}

public class Args
{
    [Obsolete]
    [DataMember(Order = 2)]
    public int Data { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }
    }
}
