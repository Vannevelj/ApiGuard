﻿using ApiGuard.Exceptions;
using System.Threading.Tasks;
using Xunit;

namespace ApiGuard.Tests
{
    public partial class ReflectionTypeLoaderTests
    {
        public class EndpointChanges
        {
            public class DifferentEndpointReturnType
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
                        public bool FirstMethod() { return true; }
                    }
                }

                [Fact]
                public async Task Differences()
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

            public class DifferentApi
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
                    public class MyNewApi
                    {
                        public int FirstMethod() { return 32; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyNewApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyNewApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected MyApi but received MyNewApi", ex.Message);
                }
            }

            public class DifferentName
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
                        public int SecondMethod() { return 32; }
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

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element int MyApi.FirstMethod() was removed", ex.Message);
                }
            }

            public class DifferentName_OnInterface
            {
                public class Before
                {
                    public interface MyApi
                    {
                        int FirstMethod();
                    }
                }

                public class After
                {
                    public interface MyApi
                    {
                        int SecondMethod();
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

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element int MyApi.FirstMethod() was removed", ex.Message);
                }
            }

            public class IdenticalTopLevelEndpoints
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
                        public int FirstMethod() { return 99; }
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
                    await Compare(typeof(Before.MyApi), typeof(After.MyApi));
                }
            }

            public class AdditionalEndpoint
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
                        public int FirstMethod() { return 32; }
                        public bool SecondMethod() { return true; }
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
                    await Compare(typeof(Before.MyApi), typeof(After.MyApi));
                }
            }

            public class AdditionalEndpoint_OnInterface
            {
                public class Before
                {
                    public interface MyApi
                    {
                        int FirstMethod();
                    }
                }

                public class After
                {
                    public interface MyApi
                    {
                        int FirstMethod();
                        bool SecondMethod();
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

                    Assert.IsType<MemberAddedToInterfaceException>(ex);
                    Assert.Equal("A mismatch on the API was found. A member was added to the interface of MyApi", ex.Message);
                }
            }

            public class AdditionalEndpoint_OnAbstractClass
            {
                public class Before
                {
                    public abstract class MyApi
                    {
                        public abstract int FirstMethod();
                    }
                }

                public class After
                {
                    public abstract class MyApi
                    {
                        public abstract int FirstMethod();
                        public abstract bool SecondMethod();
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

                    Assert.IsType<MemberAddedToInterfaceException>(ex);
                    Assert.Equal("A mismatch on the API was found. A member was added to the interface of MyApi", ex.Message);
                }
            }

            public class AdditionalEndpoint_WithChange
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
                        public double FirstMethod() { return 32.0; }
                        public bool SecondMethod() { return true; }
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
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received double MyApi.FirstMethod()", ex.Message);
                }
            }

            public class RemovedEndpoint_WithChange
            {
                public class Before
                {
                    public class MyApi
                    {
                        public int FirstMethod() { return 32; }
                        public bool SecondMethod() { return true; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public double FirstMethod() { return 32.0; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Equal(2, differences.Count);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received double MyApi.FirstMethod()", ex.Message);
                }
            }

            public class EndPointRemoved_AndEndpointChanged
            {
                public class Before
                {
                    public class MyApi
                    {
                        public Opts FirstMethod() { return null; }
                        public void SecondMethod() { }
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
                        public NewOptions FirstMethod() { return null; }
                    }

                    public class NewOptions
                    {
                        public void DoSomething() { }
                        public string DoSomethingElse() { return null; }
                    }
                }

                [Fact]
                public async Task Test()
                {
                    var firstApi = await GetApi(typeof(Before.MyApi));
                    var secondApi = await GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Equal(2, differences.Count);
                }

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected Opts MyApi.FirstMethod() but received NewOptions MyApi.FirstMethod()", ex.Message);
                }
            }

            public class ComplexReturnType_MethodAdded
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
                        public void DoSomething() { }
                        public string DoSomethingElse() { return null; }
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
                    await Compare(typeof(Before.MyApi), typeof(After.MyApi));
                }
            }

            public class ComplexReturnType_MethodChanged
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
                        public void DoSomething(int o) { }
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
                    Assert.Equal("A mismatch on the API was found. Expected void Opts.DoSomething() but received void Opts.DoSomething(int)", ex.Message);
                }
            }
        }
    }
}