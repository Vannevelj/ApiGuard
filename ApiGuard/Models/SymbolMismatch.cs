namespace ApiGuard.Models
{
    internal class SymbolMismatch
    {
        public SymbolMismatch(MismatchReason reason, ISymbol expected, ISymbol received)
        {
            Reason = reason;
            Expected = expected;
            Received = received;
        }

        public MismatchReason Reason { get; }

        public ISymbol Expected { get; }
        public ISymbol Received { get; }
    }
}