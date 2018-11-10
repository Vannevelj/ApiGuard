using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.TestApi;
using Xunit;

namespace ApiGuard.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1() => await ApiGuard.Assert.HasNotChanged(ConfigurationOptions.Default, typeof(TestApi.MyExampleService));

        [Fact]
        public async Task Test2() => await ApiGuard.Assert.HasNotChanged(ConfigurationOptions.Default, typeof(MyOtherService), typeof(TheThirdService));
    }
}
