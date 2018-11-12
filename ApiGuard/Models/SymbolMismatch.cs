namespace ApiGuard.Models
{
    internal class SymbolMismatch
    {
        public SymbolMismatch(ISymbol expected, ISymbol received)
        {
            Expected = expected;
            Received = received;
        }

        public ISymbol Expected { get; }
        public ISymbol Received { get; }
    }
}