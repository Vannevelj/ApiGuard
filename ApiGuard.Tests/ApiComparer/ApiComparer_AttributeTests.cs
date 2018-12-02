using System.Threading.Tasks;
using ApiGuard.Exceptions;
using Xunit;

namespace ApiGuard.Tests.ApiComparer
{
    public partial class ApiComparerTests
    {
        [Fact]
        public async Task ApiComparer_Attributes_WithReOrderedDataMember()
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

            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

            Assert.IsType<AttributeMismatchException>(ex);
            Assert.Equal("The DataMemberAttribute attribute has changed for Args.Data (int)", ex.Message);
        }
    }
}
