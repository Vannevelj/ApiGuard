using System;
using System.Collections.Generic;
using System.Text;

namespace ApiGuard.Models
{
    internal class EndpointResult
    {
        public bool IsExactMatch { get; set; }
        public Endpoint Endpoint { get; set; }
    }
}
