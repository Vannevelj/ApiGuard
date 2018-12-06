using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiGuard.Domain.Strategies;
using ApiGuard.Models;

namespace ApiGuard.Tests
{
    public class BaseTest
    {
        protected string GetApiFile(string apiClass)
        {
            return $@"
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Tests
{{
    {apiClass}

    public class Startup
    {{
        public static void Main(string[] args) {{ }}
    }}
}}
";
        }

        internal List<EndpointResult> GetApiDifferences(MyType originalApi, MyType newApi)
        {
            var strategy = new BestGuessEndpointMatchingStrategy();
            var apiComparer = new Domain.ApiComparer(strategy);
            return apiComparer.GetEndpointDifferences(originalApi, newApi).ToList();
        }
    }
}
