﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Domain.Strategies;
using ApiGuard.Exceptions;
using ApiGuard.Models;
using Xunit;

namespace ApiGuard.Tests
{
    public class ApiComparerTests
    {
        private readonly IApiComparer _apiComparer = new ApiComparer();

        private async Task<Api> GetApi(string source)
        {
            var symbolProvider = new SourceCodeRoslynSymbolProvider();
            var typeLoader = new RoslynTypeLoader(symbolProvider);
            return await typeLoader.LoadApi(source);
        }

        private async Task Compare(string originalApi, string newApi)
        {
            var firstApi = await GetApi(originalApi);
            var secondApi = await GetApi(newApi);

            _apiComparer.Compare(firstApi, secondApi);
        }

        [Fact]
        public async Task ApiComparer_DifferentApi()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
";

            var newApi = @"
public class MyNewApi
{
    public int FirstMethod() { return 32; }
}
";
            
            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

            Assert.IsType<ApiNotFoundException>(ex);
            Assert.Equal("Unable to find the API for type MyApi", ex.Message);
        }

        [Fact]
        public async Task ApiComparer_EndpointRemoved()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
";

            var newApi = @"
public class MyApi
{
}
";

            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

            Assert.IsType<EndpointNotFoundException>(ex);
            Assert.Equal("The API has changed. Unable to find endpoint FirstMethod() on the interface of API MyApi", ex.Message);
        }

        [Fact]
        public async Task ApiComparer_EndpointRemoved_WithParameters()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod(int x, string y) { return 32; }
}
";

            var newApi = @"
public class MyApi
{
}
";

            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

            Assert.IsType<EndpointNotFoundException>(ex);
            Assert.Equal("The API has changed. Unable to find endpoint FirstMethod(int, string) on the interface of API MyApi", ex.Message);
        }

        [Fact]
        public async Task ApiComparer_EndpointRemoved_WithOtherEndpoints()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
";

            var newApi = @"
public class MyApi
{
    public void SomeOtherMethod() { }
}
";

            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

            Assert.IsType<EndpointNotFoundException>(ex);
            Assert.Equal("The API has changed. Unable to find endpoint FirstMethod() on the interface of API MyApi", ex.Message);
        }

        [Fact]
        public async Task ApiComparer_EndpointRenamed()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
";

            var newApi = @"
public class MyApi
{
    public int NewFirstMethod() { return 32; }
}
";

            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

            Assert.IsType<EndpointNotFoundException>(ex);
            Assert.Equal("The API has changed. Endpoint FirstMethod() on MyApi is now defined as NewFirstMethod()", ex.Message);
        }

        [Fact]
        public async Task ApiComparer_EndpointRenamed_WithOtherEndpoints()
        {
            var originalApi = @"
public class MyApi
{
    public int FirstMethod() { return 32; }
}
";

            var newApi = @"
public class MyApi
{
    public int NewFirstMethod() { return 32; }
    public void SomeOtherMethod() { }
    public string YetAnotherMethod(string a) { return null; }
}
";

            var ex = await Record.ExceptionAsync(() => Compare(originalApi, newApi));

            Assert.IsType<EndpointNotFoundException>(ex);
            Assert.Equal("The API has changed. Endpoint FirstMethod() on MyApi is now defined as NewFirstMethod()", ex.Message);
        }
    }
}