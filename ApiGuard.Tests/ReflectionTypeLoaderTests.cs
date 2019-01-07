using ApiGuard.Domain;
using ApiGuard.Exceptions;
using ApiGuard.Models;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace ApiGuard.Tests
{
    public class ReflectionTypeLoaderTests : BaseTest
    {
        internal static async Task<MyType> GetApi(Type type)
        {
            var typeLoader = new ReflectionTypeLoader();
            return await typeLoader.LoadApi(type);
        }

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
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);

                Assert.Single(differences);
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
        }

        public class ApiType_FromClass_ToAbstractClass
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
                public abstract class MyApi
                {
                    public int FirstMethod() { return 32; }
                }
            }

            [Fact]
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);

                // Added the implicit "abstract" modifier and changed typekind
                Assert.Equal(2, differences.Count);
            }
        }

        public class ApiType_FromClass_ToInterface
        {
            public class Before
            {
                public class MyApi
                {
                }
            }

            public class After
            {
                public interface MyApi
                {
                }
            }

            [Fact]
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);

                // Added the implicit "abstract" modifier and changed typekind
                Assert.Equal(2, differences.Count);
            }
        }

        public class ApiType_FromAbstractClass_ToInterface
        {
            public class Before
            {
                public abstract class MyApi
                {
                }
            }

            public class After
            {
                public interface MyApi
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
        }

        public class ApiType_FromInterface_ToAbstractClass
        {
            public class Before
            {
                public interface MyApi
                {
                }
            }

            public class After
            {
                public abstract class MyApi
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
        }

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
        }

        public class AttributeValueChanged
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod(Args a) { }
                }

                public class Args
                {
                    [DataMember(Order = 1)]
                    public int Data { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod(Args a) { }
                }

                public class Args
                {
                    [DataMember(Order = 2)]
                    public int Data { get; set; }
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
        }

        public class AttributeRemoved
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod(Args a) { }
                }

                public class Args
                {
                    [DataMember(Order = 1)]
                    public int Data { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod(Args a) { }
                }

                public class Args
                {
                    public int Data { get; set; }
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
        }

        public class AttributeRemoved_OnMethod
        {
            public class Before
            {
                public class MyApi
                {
                    [Obsolete]
                    public void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod() { }
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
        }

        public class AttributeAdded
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod(Args a) { }
                }

                public class Args
                {
                    public int Data { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod(Args a) { }
                }

                public class Args
                {
                    [DataMember(Order = 1)]
                    public int Data { get; set; }
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
        }

        public class AttributeAdded_OnApiType
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                }
            }

            public class After
            {
                [Obsolete]
                public class MyApi
                {
                    public void FirstMethod() { }
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
        }

        public class AttributeValueAdded
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod(Args a) { }
                }

                public class Args
                {
                    [DataMember(Order = 1)]
                    public int Data { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod(Args a) { }
                }

                public class Args
                {
                    [DataMember(Order = 1, Name = "MyData")]
                    public int Data { get; set; }
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
        }

        public class SwappedMethod_NoChange
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                    public void SecondMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void SecondMethod() { }
                    public void FirstMethod() { }
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
        }

        public class SwappedMethod_WithChange
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                    public void SecondMethod(int a) { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void SecondMethod(string a) { }
                    public void FirstMethod() { }
                }
            }

            [Fact]
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);

                // Changed name of return type and typekind
                Assert.Equal(2, differences.Count);
            }
        }

        public class SwappedProperty_NoChange
        {
            public class Before
            {
                public class MyApi
                {
                    public int FirstProperty { get; set; }
                    public int SecondProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public int SecondProperty { get; set; }
                    public int FirstProperty { get; set; }
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
        }

        public class SwappedProperty_WithChange
        {
            public class Before
            {
                public class MyApi
                {
                    public double FirstProperty { get; set; }
                    public int SecondProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public int SecondProperty { get; set; }
                    public int FirstProperty { get; set; }
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
        }

        public class SwappedAttribute_NoChange
        {
            public class Before
            {
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
            }

            public class After
            {
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
            }

            [Fact]
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);

                Assert.Empty(differences);
            }
        }

        public class SwappedAttribute_WithChange
        {
            public class Before
            {
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
            }

            public class After
            {
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
            }

            [Fact]
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);

                Assert.Single(differences);
            }
        }

        public class InternalDeclaredAccessibilityApi
        {
            public class Before
            {
                internal class MyApi
                {
                    public void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                }
            }

            [Fact]
            public async Task Test()
            {
                var ex = await Record.ExceptionAsync(() => GetApi(typeof(Before.MyApi)));

                Assert.IsType<ApiNotPublicException>(ex);
                Assert.Equal("The type MyApi has to be public", ex.Message);

                var secondApi = await GetApi(typeof(After.MyApi));
            }
        }

        public class InternalMethod_ToPublic
        {
            public class Before
            {
                public class MyApi
                {
                    internal void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod() { }
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
        }

        public class InternalMethod_ToPrivate
        {
            public class Before
            {
                public class MyApi
                {
                    internal void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    private void FirstMethod() { }
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
        }

        public class InternalMethod_ToProtected
        {
            public class Before
            {
                public class MyApi
                {
                    internal void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    protected void FirstMethod() { }
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
        }

        public class InternalNestedType_ToPublic
        {
            public class Before
            {
                public class MyApi
                {
                    internal void FirstMethod(Args a) { }
                }

                internal class Args { }
            }

            public class After
            {
                public class MyApi
                {
                    internal void FirstMethod(Args a) { }
                }

                public class Args { }
            }

            [Fact]
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);

                Assert.Empty(differences);
            }
        }

        public class PublicMethod_ToInternal
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    internal void FirstMethod() { }
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
        }

        public class PublicMethod_ToPrivate
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    private void FirstMethod() { }
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
        }

        public class PublicMethod_ToProtected
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    protected void FirstMethod() { }
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
        }

        public class PrivateMethod_WithChange
        {
            public class Before
            {
                public class MyApi
                {
                    private void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    private void FirstMethod(string a) { }
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
        }

        public class ProtectedMethod_WithChange
        {
            public class Before
            {
                public class MyApi
                {
                    protected void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    protected void FirstMethod(string a) { }
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
        }

        public class InternalMethod_WithChange
        {
            public class Before
            {
                public class MyApi
                {
                    internal void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    internal void FirstMethod(string a) { }
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
        }

        public class PrivateMethod_Added
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                    private void Nothing() { }
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
        }

        public class InstanceMethod_MadeStatic
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public static void FirstMethod() { }
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
        }

        public class StaticMethod_MadeInstance
        {
            public class Before
            {
                public class MyApi
                {
                    public static void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod() { }
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
        }

        public class VirtualMethod_MadeNonVirtual
        {
            public class Before
            {
                public class MyApi
                {
                    public virtual void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void FirstMethod() { }
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
        }

        public class NonVirtualMethod_MadeVirtual
        {
            public class Before
            {
                public class MyApi
                {
                    public void FirstMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public virtual void FirstMethod() { }
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
        }

        public class NonStaticClass_MadeStatic
        {
            public class Before
            {
                public class MyApi
                {
                }
            }

            public class After
            {
                public static class MyApi
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
        }

        public class StaticClass_MadeNonStatic
        {
            public class Before
            {
                public static class MyApi
                {

                }
            }

            public class After
            {
                public class MyApi
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
        }

        public class AbstractMethod_MadeNonAbstract
        {
            public class Before
            {
                public abstract class MyApi
                {
                    public abstract void MyMethod();
                }
            }

            public class After
            {
                public abstract class MyApi
                {
                    public void MyMethod()
                    {
                    }
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
        }

        public class NonAbstractMethod_MadeAbstract
        {
            public class Before
            {
                public abstract class MyApi
                {
                    public void MyMethod()
                    {
                    }
                }
            }

            public class After
            {
                public abstract class MyApi
                {
                    public abstract void MyMethod();
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
        }

        public class NonAbstractType_MadeAbstract
        {
            public class Before
            {
                public class MyApi
                {
                }
            }

            public class After
            {
                public abstract class MyApi
                {
                }
            }

            [Fact]
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);

                // Changed modifiers and type kind
                Assert.Equal(2, differences.Count);
            }
        }

        public class AbstractType_MadeNonAbstract
        {
            public class Before
            {
                public abstract class MyApi
                {
                }
            }

            public class After
            {
                public class MyApi
                {
                }
            }

            [Fact]
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);

                // This records a change in modifiers as well as the type
                Assert.Equal(2, differences.Count);
            }
        }

        public class SealedType_MadeNonSealed
        {
            public class Before
            {
                public sealed class MyApi
                {
                }
            }

            public class After
            {
                public class MyApi
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
        }

        public class NonSealedType_MadeSealed
        {
            public class Before
            {
                public class MyApi
                {
                }
            }

            public class After
            {
                public sealed class MyApi
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
        }

        public class ImplicitlyDeclaredAccessibilityMethod_MadePublic
        {
            public class Before
            {
                public class MyApi
                {
                    void MyMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public void MyMethod() { }
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
        }

        public class ImplicitlyDeclaredAccessibilityMethod_MadePrivate
        {
            public class Before
            {
                public class MyApi
                {
                    void MyMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    private void MyMethod() { }
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
        }

        public class ImplicitlyDeclaredAccessibilityMethod_MadeInternal
        {
            public class Before
            {
                public class MyApi
                {
                    void MyMethod() { }
                }
            }

            public class After
            {
                public class MyApi
                {
                    internal void MyMethod() { }
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
        }

        public class ImplicitlyDeclaredAccessibilityProperty_MadePublic
        {
            public class Before
            {
                public class MyApi
                {
                    string MyProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public string MyProperty { get; set; }
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
        }

        public class PublicInterface_MadeInternal
        {
            public class Before
            {
                public class MyApi
                {
                    public IComparable MyProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    internal IComparable MyProperty { get; set; }
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
        }

        public class PublicProperty_MadeImplicit
        {
            public class Before
            {
                public class MyApi
                {
                    public string MyProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    string MyProperty { get; set; }
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
        }

        public class PublicProperty_WithChange
        {
            public class Before
            {
                public class MyApi
                {
                    public string MyProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    public int MyProperty { get; set; }
                }
            }

            [Fact]
            public async Task Test()
            {
                var firstApi = await GetApi(typeof(Before.MyApi));
                var secondApi = await GetApi(typeof(After.MyApi));

                var differences = GetApiDifferences(firstApi, secondApi);
                
                // Changed name of return type and typekind
                Assert.Equal(2, differences.Count);
            }
        }

        public class PublicProperty_MadeInternal
        {
            public class Before
            {
                public class MyApi
                {
                    public string MyProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    internal string MyProperty { get; set; }
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
        }

        public class PublicProperty_MadeProtected
        {
            public class Before
            {
                public class MyApi
                {
                    public string MyProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    protected string MyProperty { get; set; }
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
        }

        public class PublicProperty_MadePrivate
        {
            public class Before
            {
                public class MyApi
                {
                    public string MyProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    private string MyProperty { get; set; }
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
        }

        public class PrivateProperty_WithChange
        {
            public class Before
            {
                public class MyApi
                {
                    private string MyProperty { get; set; }
                }
            }

            public class After
            {
                public class MyApi
                {
                    private int MyProperty { get; set; }
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
        }
    }
}
