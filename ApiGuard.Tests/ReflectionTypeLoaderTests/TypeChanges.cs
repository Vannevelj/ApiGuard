using ApiGuard.Exceptions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ApiGuard.Tests
{
    public partial class ReflectionTypeLoaderTests
    {
        public class TypeChanges
        {
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

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<TypeKindChangedException>(ex);
                    Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
                }
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<TypeKindChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. The type of MyApi was changed", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<TypeKindChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. The type of MyApi was changed", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<TypeKindChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. The type of MyApi was changed", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

                Assert.Single(differences);
            }

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<DefinitionMismatchException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected void MyApi.SecondMethod(int) but received void MyApi.SecondMethod(string)", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<DefinitionMismatchException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected MyApi.FirstProperty (double) but received MyApi.FirstProperty (int)", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ApiNotPublicException>(ex);
                Assert.Equal("The type MyApi has to be public", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<DefinitionMismatchException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected void MyApi.FirstMethod() but received void MyApi.FirstMethod(string)", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. A modifier changed on void MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. A modifier changed on void MyApi.FirstMethod()", ex.Message);
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

                Assert.Equal(2, differences.Count);
            }

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. A modifier changed on MyApi", ex.Message);
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

                Assert.Equal(2, differences.Count);
            }

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. A modifier changed on void MyApi.MyMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. A modifier changed on void MyApi.MyMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. A modifier changed on MyApi", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<TypeKindChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. The type of MyApi was changed", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. A modifier changed on MyApi", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. A modifier changed on MyApi.MyProperty (IComparable)", ex.Message);
            }
        }

        public class PublicProperty_MadeImplicitlyPrivate
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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
                
                Assert.Single(differences);
            }

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<DefinitionMismatchException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected MyApi.MyProperty (string) but received MyApi.MyProperty (int)", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                Assert.IsType<ModifierChangedException>(ex);
                Assert.Equal("A mismatch on the API was found. Expected int MyApi.FirstMethod() but received bool MyApi.FirstMethod()", ex.Message);
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

            [Fact]
            public async Task CompareResult()
            {
                await Compare(typeof(Before.MyApi), typeof(After.MyApi));
            }
        }
    }
}
