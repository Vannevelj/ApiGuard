using System.Collections.Generic;

namespace ApiGuard.Models
{
    internal class EndpointResult
    {
        public bool IsExactMatch => Differences == 0;
        public int Differences { get; set; }
        public MyMethod ExistingEndpoint { get; set; }
        public MyMethod ReceivedEndpoint { get; set; }
        public List<SymbolMismatch> SymbolsChanged { get; set; }
    }
}
