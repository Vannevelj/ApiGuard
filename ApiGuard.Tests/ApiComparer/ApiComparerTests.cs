//using System.Threading.Tasks;
//using ApiGuard.Domain;
//using ApiGuard.Domain.Interfaces;
//using ApiGuard.Domain.Strategies;
//using ApiGuard.Exceptions;
//using ApiGuard.Models;
//using Xunit;

//namespace ApiGuard.Tests.ApiComparer
//{
//    public partial class ApiComparerTests : BaseTest
//    {
//        private readonly IApiComparer _apiComparer;

//        public ApiComparerTests()
//        {
//            var strategy = new BestGuessEndpointMatchingStrategy();
//            _apiComparer = new Domain.ApiComparer(strategy);
//        }

//        private async Task<MyType> GetApi(string source)
//        {
//            var symbolProvider = new SourceCodeRoslynSymbolProvider();
//            var typeLoader = new RoslynTypeLoader(symbolProvider);
//            return await typeLoader.LoadApi(source);
//        }

//        private async Task Compare(string originalApi, string newApi)
//        {
//            var firstApi = await GetApi(originalApi);
//            var secondApi = await GetApi(newApi);

//            _apiComparer.Compare(firstApi, secondApi);
//        }

//        [Fact]
//        public async Task ApiComparer_EndpointRemoved_SingleEndpoint()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod() { return 32; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element int MyApi.FirstMethod() was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_EndpointRemoved_WithParameters()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(int x, string y) { return 32; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element int MyApi.FirstMethod(int, string) was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_EndpointRemoved_WithOtherEndpoints()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod() { return 32; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void SomeOtherMethod() { }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element int MyApi.FirstMethod() was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_EndpointRenamed()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod() { return 32; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public int NewFirstMethod() { return 32; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element int MyApi.FirstMethod() was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_EndpointRenamed_WithOtherEndpoints()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod() { return 32; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public int NewFirstMethod() { return 32; }
//    public void SomeOtherMethod() { }
//    public string YetAnotherMethod(string a) { return null; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element int MyApi.FirstMethod() was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_ParameterChanged_ComplexObject_NameChanged()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(Args a) { return 32; }
//}

//public class Args
//{
//    public string X { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(Args a) { return 32; }
//}

//public class Args
//{
//    public string Y { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element Args.X (string) was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_ParameterChanged_ComplexObject_TypeChanged()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(Args a) { return 32; }
//}

//public class Args
//{
//    public string X { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(Args a) { return 32; }
//}

//public class Args
//{
//    public int X { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<DefinitionMismatchException>(ex);
//            Assert.Equal("A mismatch on the API was found. Expected Args.X (string) but received Args.X (int)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_ParameterChanged_SimpleObject_TypeChanged()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(string a) { return 32; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(int a) { return 32; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<DefinitionMismatchException>(ex);
//            Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod(string) but received int MyApi.FirstMethod(int)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_ParameterChanged_SimpleObject_NameChanged()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(string initial) { return 32; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(string newValue) { return 32; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ParameterNameMismatchException>(ex);
//            Assert.Equal("A mismatch on the API was found. The parameter on MyApi.FirstMethod was renamed from initial to newValue", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_ParameterChanged_NestedComplexObject_TypeChanged()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(Args a) { return 32; }
//}

//public class Args
//{
//    public Opts Options { get; set; }
//}

//public class Opts
//{
//    public string X { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(Args a) { return 32; }
//}

//public class Args
//{
//    public Opts Options { get; set; }
//}

//public class Opts
//{
//    public int X { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<DefinitionMismatchException>(ex);
//            Assert.Equal("A mismatch on the API was found. Expected Opts.X (string) but received Opts.X (int)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_ParameterChanged_NestedComplexObject_TypeChanged_AsInterface()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(Args a) { return 32; }
//}

//public interface Args
//{
//    Opts Options { get; set; }
//}

//public class Opts
//{
//    public string X { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public int FirstMethod(Args a) { return 32; }
//}

//public interface Args
//{
//    Opts Options { get; set; }
//}

//public class Opts
//{
//    public int X { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<DefinitionMismatchException>(ex);
//            Assert.Equal("A mismatch on the API was found. Expected Opts.X (string) but received Opts.X (int)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_EndpointReturnTypeChanged_NestedComplexObject_TypeChanged()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public Data Data { get; set; }
//}

//public class Data
//{
//    public string X { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public Data Data { get; set; }
//}

//public class Data
//{
//    public int X { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<DefinitionMismatchException>(ex);
//            Assert.Equal("A mismatch on the API was found. Expected Data.X (string) but received Data.X (int)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_EndpointReturnTypeChanged_ComplexObject_TypeChanged()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public string X { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public double X { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<DefinitionMismatchException>(ex);
//            Assert.Equal("A mismatch on the API was found. Expected Result.X (string) but received Result.X (double)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_MultipleChangesInSameSubtree_WithPositiveChange()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public string X { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public string NewProp { get; set; }
//    public int Y { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element Result.X (string) was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_MultipleChangesInSameSubtree_WithNegativeChange()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public string NewProp { get; set; }
//    public string X { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public int Y { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element Result.NewProp (string) was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_PropertyRemoved()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public string X { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element Result.X (string) was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_MethodRemoved()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//    public void GetResult() { }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public class Result
//{
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element void Result.GetResult() was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_MethodRemoved_AsInterface()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public interface Result
//{
//    void GetResult();
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//}

//public interface Result
//{
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element void Result.GetResult() was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_MultipleChangesInDifferentSubtree_WithNegativeChange()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//    public void SecondMethod() { }
//}

//public class Result
//{
//    public string Y { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//    public void SecondMethod(int a) { }
//}

//public class Result
//{
//    public int Y { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<DefinitionMismatchException>(ex);
//            Assert.Equal("A mismatch on the API was found. Expected Result.Y (string) but received Result.Y (int)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_MultipleChangesInDifferentSubtree_WithPositiveChange()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//    public void SecondMethod() { }
//}

//public class Result
//{
//    public string Y { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public Result FirstMethod() { return null; }
//    public void SecondMethod(int a) { }
//}

//public class Result
//{
//    public string Y { get; set; }
//    public int X { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<DefinitionMismatchException>(ex);
//            Assert.Equal("A mismatch on the API was found. Expected void MyApi.SecondMethod() but received void MyApi.SecondMethod(int)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_ReOrderedOverloadedMethods_NoChange()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void Method(int a, int b) { }
//    public void Method(string a, string b) { }
//    public void Method(double a, int b) { }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void Method(string a, string b) { }
//    public void Method(double a, int b) { }
//    public void Method(int a, int b) { }
//}
//");

//            await Compare(originalApi, newApi);
//        }

//        [Fact]
//        public async Task ApiComparer_ReOrderedOverloadedMethods_WithChange()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void Method(int a, int b) { }
//    public void Method(string a, string b) { }
//    public void Method(double a, int b) { }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void Method(string a, string b) { }
//    public void Method(int a, int b) { }
//    public void Method(string a, int b) { }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element void MyApi.Method(double, int) was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_ReOrderedOverloadedMethods_WithDelete()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void Method(int a, int b) { }
//    public void Method(string a, string b) { }
//    public void Method(double a, int b) { }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void Method(string a, string b) { }
//    public void Method(int a, int b) { }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element void MyApi.Method(double, int) was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_ReOrderedOverloadedMethods_WithDelete_AsInterface()
//        {
//            var originalApi = GetApiFile(@"
//public interface MyApi
//{
//    void Method(int a, int b);
//    void Method(string a, string b);
//    void Method(double a, int b);
//}
//");

//            var newApi = GetApiFile(@"
//public interface MyApi
//{
//    void Method(string a, string b);
//    void Method(int a, int b);
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element void MyApi.Method(double, int) was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_FromAbstractClass_ToInterface()
//        {
//            var originalApi = GetApiFile(@"
//public abstract class MyApi
//{
//}
//");

//            var newApi = GetApiFile(@"
//public interface MyApi
//{
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<TypeKindChangedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The type of MyApi was changed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_FromInterface_ToAbstractClass()
//        {
//            var originalApi = GetApiFile(@"
//public interface MyApi
//{
//}
//");

//            var newApi = GetApiFile(@"
//public abstract class MyApi
//{
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<TypeKindChangedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The type of MyApi was changed", ex.Message);
//        }

//        [Fact]
//        public async Task AdditionalEndpoint_OnInterface()
//        {
//            var originalApi = GetApiFile(@"
//public interface MyApi
//{
//    int FirstMethod();
//}
//");

//            var newApi = GetApiFile(@"
//public interface MyApi
//{
//    int FirstMethod();
//    bool SecondMethod();
//}
//");
//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<MemberAddedToInterfaceException>(ex);
//            Assert.Equal("A mismatch on the API was found. A member was added to the interface of MyApi", ex.Message);
//        }

//        [Fact]
//        public async Task AdditionalEndpoint_OnAbstractClass()
//        {
//            var originalApi = GetApiFile(@"
//public abstract class MyApi
//{
//    public abstract int FirstMethod();
//}
//");

//            var newApi = GetApiFile(@"
//public abstract class MyApi
//{
//    public abstract int FirstMethod();
//    public abstract bool SecondMethod();
//}
//");
//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<MemberAddedToInterfaceException>(ex);
//            Assert.Equal("A mismatch on the API was found. A member was added to the interface of MyApi", ex.Message);
//        }
//    }
//}
