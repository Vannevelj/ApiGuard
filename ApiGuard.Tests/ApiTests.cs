using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.Domain.Strategies;
using ApiGuard.Exceptions;
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
        public async Task DifferentName_OnInterface()
        {
            var originalApi = GetApiFile(@"
public interface MyApi
{
    int FirstMethod();
}
");

            var newApi = GetApiFile(@"
public interface MyApi
{
    int SecondMethod();
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
        public async Task DifferentParameterType_OnInterface()
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
    public int FirstMethod(IComparable c) { return 32; }
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

            Assert.Equal(3, differences.Count);
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

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
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

            Assert.Single(differences);
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

        [Fact]
        public async Task InternalDeclaredAccessibilityApi()
        {
            var originalApi = GetApiFile(@"
internal class MyApi
{
    public void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
}
");
            var ex = await Record.ExceptionAsync(() => GetApi(originalApi));

            Assert.IsType<ApiNotPublicException>(ex);
            Assert.Equal("The type MyApi has to be public", ex.Message);

            var secondApi = await GetApi(newApi);
        }

        [Fact]
        public async Task InternalMethod_ToPublic()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    internal void FirstMethod() { }
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

            Assert.Empty(differences);
        }

        [Fact]
        public async Task InternalMethod_ToPrivate()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    internal void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    private void FirstMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task InternalMethod_ToProtected()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    internal void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    protected void FirstMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task InternalNestedType_ToPublic()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    internal void FirstMethod(Args a) { }
}

internal class Args { }
");

            var newApi = GetApiFile(@"
public class MyApi
{
    internal void FirstMethod(Args a) { }
}

public class Args { }
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task PublicMethod_ToInternal()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    internal void FirstMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task PublicMethod_ToPrivate()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    private void FirstMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task PublicMethod_ToProtected()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    protected void FirstMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task PrivateMethod_WithChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    private void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    private void FirstMethod(string a) { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ProtectedMethod_WithChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    protected void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    protected void FirstMethod(string a) { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task InternalMethod_WithChange()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    internal void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    internal void FirstMethod(string a) { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task PrivateMethod_Added()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
    private void Nothing() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task InstanceMethod_MadeStatic()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public static void FirstMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task StaticMethod_MadeInstance()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public static void FirstMethod() { }
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
        public async Task VirtualMethod_MadeNonVirtual()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public virtual void FirstMethod() { }
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
        public async Task NonVirtualMethod_MadeVirtual()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public void FirstMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public virtual void FirstMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task NonStaticClass_MadeStatic()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
}
");

            var newApi = GetApiFile(@"
public static class MyApi
{
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task StaticClass_MadeNonStatic()
        {
            var originalApi = GetApiFile(@"
public static class MyApi
{

}
");

            var newApi = GetApiFile(@"
public class MyApi
{

}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task AbstractMethod_MadeNonAbstract()
        {
            var originalApi = GetApiFile(@"
public abstract class MyApi
{
    public abstract void MyMethod();
}
");

            var newApi = GetApiFile(@"
public abstract class MyApi
{
    public void MyMethod() 
    {
    }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task NonAbstractMethod_MadeAbstract()
        {
            var originalApi = GetApiFile(@"
public abstract class MyApi
{
    public void MyMethod() 
    {
    }
}
");

            var newApi = GetApiFile(@"
public abstract class MyApi
{
    public abstract void MyMethod();
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task NonAbstractType_MadeAbstract()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
}
");

            var newApi = GetApiFile(@"
public abstract class MyApi
{
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task AbstractType_MadeNonAbstract()
        {
            var originalApi = GetApiFile(@"
public abstract class MyApi
{
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task SealedType_MadeNonSealed()
        {
            var originalApi = GetApiFile(@"
public sealed class MyApi
{
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task NonSealedType_MadeSealed()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
}
");

            var newApi = GetApiFile(@"
public sealed class MyApi
{
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task ImplicitlyDeclaredAccessibilityMethod_MadePublic()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    void MyMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void MyMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ImplicitlyDeclaredAccessibilityMethod_MadePrivate()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    void MyMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    private void MyMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ImplicitlyDeclaredAccessibilityMethod_MadeInternal()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    void MyMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    internal void MyMethod() { }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task ImplicitlyDeclaredAccessibilityApi()
        {
            var originalApi = GetApiFile(@"
class MyApi
{
    public void MyMethod() { }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public void MyMethod() { }
}
");
            var ex = await Record.ExceptionAsync(() => GetApi(originalApi));

            Assert.IsType<ApiNotPublicException>(ex);
            Assert.Equal("The type MyApi has to be public", ex.Message);

            var secondApi = await GetApi(newApi);
        }

        [Fact]
        public async Task ImplicitlyDeclaredAccessibilityProperty_MadePublic()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    string MyProperty { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    public string MyProperty { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Empty(differences);
        }

        [Fact]
        public async Task PublicProperty_MadeImplicit()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public string MyProperty { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    string MyProperty { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task PublicProperty_MadeInternal()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public string MyProperty { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    internal string MyProperty { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task PublicProperty_MadeProtected()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public string MyProperty { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    protected string MyProperty { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }

        [Fact]
        public async Task PublicProperty_MadePrivate()
        {
            var originalApi = GetApiFile(@"
public class MyApi
{
    public string MyProperty { get; set; }
}
");

            var newApi = GetApiFile(@"
public class MyApi
{
    private string MyProperty { get; set; }
}
");
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            var differences = GetApiDifferences(firstApi, secondApi);

            Assert.Single(differences);
        }
    }
}
