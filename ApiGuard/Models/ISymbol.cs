namespace ApiGuard.Models
{
    internal interface ISymbol
    {
        int Depth { get; set; }
        string Name { get; set; }
        ISymbol Parent { get; set; }
    }
}