using System;
using System.Collections.Generic;
using System.Text;

namespace ApiGuard.Models
{
    internal class EndpointResult
    {
        public bool IsExactMatch => Differences == 0;
        public int Differences { get; set; }
        public MyMethod Endpoint { get; set; }
        public List<SymbolMismatch> SymbolsChanged { get; set; }
    }
}
