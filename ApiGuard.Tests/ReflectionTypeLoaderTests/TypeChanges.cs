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

                Assert.Equal(2, differences.Count);
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
