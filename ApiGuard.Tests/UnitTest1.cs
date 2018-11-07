using System.Threading.Tasks;
using ApiGuard.TestApi;
using Xunit;

namespace ApiGuard.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1() => await ApiGuard.Assert.HasNotChanged(typeof(Api));
    }
}
