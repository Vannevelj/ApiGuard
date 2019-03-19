using ApiGuard.Domain;
using System;
using System.Collections.Generic;
using Xunit;

namespace ApiGuard.Tests
{
    public class ProjectResolverTests
    {
        [Fact]
        public void CanRoundtripDeserializeApi()
        {
            var resolver = new ProjectResolver();
            var typeLoader = new ReflectionTypeLoader();
            var api = typeLoader.LoadApi(typeof(TestApi));

            var serializedApi = resolver.SerializeApi(api);
            var deserializedApi = resolver.DeserializeApi(serializedApi);

            Assert.Equal(api, deserializedApi);
        }

        public class TestApi
        {
            public School School { get; set; }
        }

        public class School
        {
            public List<string> Students { get; set; }

            public string Name { get; set; }

            public DateTime Founded { get; set; }

            public DateTime? Closed { get; set; }
        }
    }
}
