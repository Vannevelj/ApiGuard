using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
