using System.Threading.Tasks;
using ApiGuard.TestApi;
using Xunit;

namespace ApiGuard.TestApiTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1() => ApiGuard.ApiAssert.HasNotChanged(typeof(MyExampleService));

        [Fact]
        public void Test2() => ApiGuard.ApiAssert.HasNotChanged(typeof(MyOtherService), typeof(TheThirdService));
    }
}
