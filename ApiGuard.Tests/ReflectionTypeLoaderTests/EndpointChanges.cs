using ApiGuard.Exceptions;
using System;
using System.Collections.Generic;
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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyNewApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyNewApi)));

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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element int MyApi.FirstMethod() was removed", ex.Message);
                }
            }

            public class DifferentName_AsParameterInAList
            {
                public class Before
                {
                    public class MyApi
                    {
                        public List<MyObject> Objects { get; set; }
                    }

                    public class MyObject
                    {
                       public string Name { get; set; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public List<MyObject> Objects { get; set; }
                    }

                    public class MyObject
                    {
                        public string NewName { get; set; }
                    }
                }

                [Fact]
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element MyObject.Name (string) was removed", ex.Message);
                }
            }

            public class DifferentName_AsParameterInADictionary
            {
                public class Before
                {
                    public class MyApi
                    {
                        public Dictionary<string, MyObject> Objects { get; set; }
                    }

                    public class MyObject
                    {
                        public string Name { get; set; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public Dictionary<string, MyObject> Objects { get; set; }
                    }

                    public class MyObject
                    {
                        public string NewName { get; set; }
                    }
                }

                [Fact]
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element MyObject.Name (string) was removed", ex.Message);
                }
            }

            public class DifferentName_OnStruct
            {
                public class Before
                {
                    public class MyApi
                    {
                        public DateTime SomeDate { get; set; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public DateTime SomeNewDate { get; set; }
                    }
                }

                [Fact]
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element MyApi.SomeDate (DateTime) was removed", ex.Message);
                }
            }

            public class DifferentName_OnNullableStruct
            {
                public class Before
                {
                    public class MyApi
                    {
                        public DateTime? SomeDate { get; set; }
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public DateTime? SomeNewDate { get; set; }
                    }
                }

                [Fact]
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element MyApi.SomeDate (DateTime?) was removed", ex.Message);
                }
            }

            public class DifferentName_OnReadonlyProperty
            {
                public class Before
                {
                    public class MyApi
                    {
                        public DateTime SomeDate => DateTime.UtcNow;
                    }
                }

                public class After
                {
                    public class MyApi
                    {
                        public DateTime SomeNewDate => DateTime.UtcNow;
                    }
                }

                [Fact]
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element MyApi.SomeDate (DateTime) was removed", ex.Message);
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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Empty(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    Compare(typeof(Before.MyApi), typeof(After.MyApi));
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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Empty(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    Compare(typeof(Before.MyApi), typeof(After.MyApi));
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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Equal(2, differences.Count);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element bool MyApi.SecondMethod() was removed", ex.Message);
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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Equal(2, differences.Count);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element void MyApi.SecondMethod() was removed", ex.Message);
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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Empty(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    Compare(typeof(Before.MyApi), typeof(After.MyApi));
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
                public void CompareDifferences()
                {
                    var firstApi = GetApi(typeof(Before.MyApi));
                    var secondApi = GetApi(typeof(After.MyApi));

                    var differences = GetApiDifferences(firstApi, secondApi);

                    Assert.Single(differences);
                }

                [Fact]
                public void CompareResult()
                {
                    var ex = Record.Exception(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<DefinitionMismatchException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected void Opts.DoSomething() but received void Opts.DoSomething(int)", ex.Message);
                }
            }
        }
    }
}
