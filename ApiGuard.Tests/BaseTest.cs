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

        internal List<EndpointResult> GetApiDifferences(Api originalApi, Api newApi)
        {
            var matchingStrategy = new BestGuessEndpointMatchingStrategy();
            return originalApi.GetEndpointDifferences(newApi, matchingStrategy).ToList();
        }
    }
}
