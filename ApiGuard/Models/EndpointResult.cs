using System.Collections.Generic;

namespace ApiGuard.Models
{
    internal class EndpointResult
    {
        public bool IsExactMatch => SymbolsChanged.Count == 0;
        public MyMethod ExistingEndpoint { get; set; }
        public MyMethod ReceivedEndpoint { get; set; }
        public List<SymbolMismatch> SymbolsChanged { get; set; }
    }
}
