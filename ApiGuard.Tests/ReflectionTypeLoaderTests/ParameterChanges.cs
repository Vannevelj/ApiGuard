using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ApiGuard.Tests
{
    public partial class ReflectionTypeLoaderTests
    {
        public class ParameterChanges
        {
            public class AdditionalParameter
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod() { return 32; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(int x) { return 32; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class DifferentParameterType
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod() { return 32; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(bool x) { return 32; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class DifferentParameterType_OnInterface
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod() { return 32; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(IComparable c) { return 32; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class DifferentParameterName
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(int abc) { return 32; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(int def) { return 32; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType
            {
                public class Before
                {
                    public class Opts
                    {
                        public string Key { get; set; }
                        public string Value { get; set; }
                    }

                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }
                }

                public class After
                {
                    public class Opts
                    {
                        public string Key { get; set; }
                        public string Value { get; set; }
                    }

                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Empty(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType_PropertyNameChanged
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public string Key { get; set; }
                        public string Value { get; set; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public string Key { get; set; }
                        public string NewValue { get; set; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType_PropertyTypeChanged
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public string Key { get; set; }
                        public string Value { get; set; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public string Key { get; set; }
                        public object Value { get; set; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType_PropertyAdded
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public string Key { get; set; }
                        public string Value { get; set; }
                    }
                }

                public class After
                {
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
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Empty(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType_PropertyRemoved
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public string Key { get; set; }
                        public string Value { get; set; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public string Key { get; set; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType_MethodNameChanged
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public void DoSomething() { }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public void DoSomethingElse() { }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType_MethodReturnTypeChanged
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public void DoSomething() { }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public Task DoSomethingElse() { return Task.CompletedTask; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType_MethodAdded
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public void DoSomething() { }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public void DoSomething() { }
                        public void DoSomethingElse() { }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Empty(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType_MethodRemoved
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public void DoSomething() { }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {

                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexParameterType_MultipleChanges
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public int Key { get; set; }
                        public string DoSomething() { return null; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public int FirstMethod(Opts o) { return 32; }
                    }

                    public class Opts
                    {
                        public void DoSomething(object o) { }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    // Removed a property
                    // Added an argument
                    // Changed the name of the return type
                    // The kind of type changed from 'class' to 'void'
                    Assert.Equal(4, differences.Count);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexReturnType_MethodRemoved
            {
                public class Before
                {
                    public class MyApi
                    {
                        public Opts FirstMethod() { return null; }
                    }

                    public class Opts
                    {
                        public void DoSomething() { }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public Opts FirstMethod() { return null; }
                    }

                    public class Opts
                    {

                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
            }
        }
    }
}
