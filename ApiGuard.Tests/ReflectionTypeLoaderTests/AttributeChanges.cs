using ApiGuard.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ApiGuard.Tests
{
    public partial class ReflectionTypeLoaderTests
    {
        public class AttributeChanges
        {
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

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<AttributeMismatchException>(ex);
                    Assert.Equal("The DataMemberAttribute attribute has changed for Args.Data (int)", ex.Message);
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

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element DataMemberAttribute was removed", ex.Message);
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

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<ElementRemovedException>(ex);
                    Assert.Equal("A mismatch on the API was found. The element ObsoleteAttribute was removed", ex.Message);
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

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<AttributeMismatchException>(ex);
                    Assert.Equal("The DataMemberAttribute attribute has changed for Args.Data (int)", ex.Message);
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

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<AttributeMismatchException>(ex);
                    Assert.Equal("The ObsoleteAttribute attribute has changed for MyApi", ex.Message);
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

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<AttributeMismatchException>(ex);
                    Assert.Equal("The DataMemberAttribute attribute has changed for Args.Data (int)", ex.Message);
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

                [Fact]
                public async Task CompareResult()
                {
                    await Compare(typeof(Before.MyApi), typeof(After.MyApi));
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

                [Fact]
                public async Task CompareResult()
                {
                    var ex = await Record.ExceptionAsync(() => Compare(typeof(Before.MyApi), typeof(After.MyApi)));

                    Assert.IsType<AttributeMismatchException>(ex);
                    Assert.Equal("The DataMemberAttribute attribute has changed for Args.Data (int)", ex.Message);
                }
            }
        }
    }
}
