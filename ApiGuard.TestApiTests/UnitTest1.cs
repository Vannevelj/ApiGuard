using System.Threading.Tasks;
using ApiGuard.TestApi;
using Xunit;

namespace ApiGuard.TestApiTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1() => await ApiGuard.ApiAssert.HasNotChanged(typeof(MyExampleService));

        [Fact]
        public async Task Test2() => await ApiGuard.ApiAssert.HasNotChanged(typeof(MyOtherService), typeof(TheThirdService));
    }
}
