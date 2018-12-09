namespace ApiGuard.Models
{
    internal interface ISymbol
    {
        string Name { get; set; }
        ISymbol Parent { get; set; }
    }
}