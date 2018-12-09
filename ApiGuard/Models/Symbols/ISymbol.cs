namespace ApiGuard.Models.Symbols
{
    internal interface ISymbol
    {
        string Name { get; set; }
        ISymbol Parent { get; set; }
    }
}