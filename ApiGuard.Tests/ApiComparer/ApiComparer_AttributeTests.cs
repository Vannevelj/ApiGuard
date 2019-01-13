//using System;
//using System.Threading.Tasks;
//using ApiGuard.Exceptions;
//using Xunit;

//namespace ApiGuard.Tests.ApiComparer
//{
//    public partial class ApiComparerTests
//    {
//        [Fact]
//        public async Task ApiComparer_Attributes_WithChangedArgument()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{
//    [DataMember(Order = 1)]
//    public int Data { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{
//    [DataMember(Order = 2)]
//    public int Data { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<AttributeMismatchException>(ex);
//            Assert.Equal("The DataMemberAttribute attribute has changed for Args.Data (int)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_Attributes_ArgumentValuesSwapped()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{
//    [DataMember(Order = 1)]
//    public int Data { get; set; }

//    [DataMember(Order = 2)]
//    public string MoreData { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{
//    [DataMember(Order = 2)]
//    public int Data { get; set; }

//    [DataMember(Order = 1)]
//    public string MoreData { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<AttributeMismatchException>(ex);
//            Assert.Equal("The DataMemberAttribute attribute has changed for Args.Data (int)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_Attributes_WithChangedArgument_OnMethod()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    [Some]
//    public void FirstMethod(double d) { }
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    [Some(Something = 5)]
//    public void FirstMethod(double d) { }
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<AttributeMismatchException>(ex);
//            Assert.Equal("The SomeAttribute attribute has changed for void MyApi.FirstMethod(double)", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_Attributes_AddedAttribute_OnApiType()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(double d) { }
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//[Some]
//public class MyApi
//{
//    public void FirstMethod(double d) { }
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<AttributeMismatchException>(ex);
//            Assert.Equal("The SomeAttribute attribute has changed for MyApi", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_Attributes_AddedAttribute_OnApiType_AsInterface()
//        {
//            var originalApi = GetApiFile(@"
//public interface MyApi
//{
//    void FirstMethod(double d);
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//[Some]
//public interface MyApi
//{
//    void FirstMethod(double d);
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<AttributeMismatchException>(ex);
//            Assert.Equal("The SomeAttribute attribute has changed for MyApi", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_Attributes_AddedAttribute_OnNestedType()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{

//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var newApi = GetApiFile(@"

//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//[Some]
//public class Args
//{

//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<AttributeMismatchException>(ex);
//            Assert.Equal("The SomeAttribute attribute has changed for Args", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_Attributes_RemovedAttribute_OnApiType()
//        {
//            var originalApi = GetApiFile(@"
//[Some]
//public class MyApi
//{
//    public void FirstMethod(double d) { }
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(double d) { }
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element SomeAttribute was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_Attributes_RemovedAttribute_OnNestedMethod()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{
//    [Obsolete]
//    public int Something { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{
//    public int Something { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element ObsoleteAttribute was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_Attributes_RemovedAttribute_OnNestedType()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//[Obsolete]
//public class Args
//{
//    public int Something { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{
//    public int Something { get; set; }
//}
//");

//            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

//            Assert.IsType<ElementRemovedException>(ex);
//            Assert.Equal("A mismatch on the API was found. The element ObsoleteAttribute was removed", ex.Message);
//        }

//        [Fact]
//        public async Task ApiComparer_Attributes_ReOrderedAttribute()
//        {
//            var originalApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{
//    [Obsolete]
//    [Some]
//    public int Something { get; set; }
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            var newApi = GetApiFile(@"
//public class MyApi
//{
//    public void FirstMethod(Args a) { }
//}

//public class Args
//{
//    [Some]
//    [Obsolete]
//    public int Something { get; set; }
//}

//public class SomeAttribute : Attribute
//{
//    public int Something { get; set; }
//}
//");

//            await Compare(originalApi, newApi);
//        }
//    }
//}
