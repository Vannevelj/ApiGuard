namespace ApiGuard.Models
{
    internal interface ISymbol
    {
        int Depth { get; set; }
    }

    internal interface IChildSymbol : ISymbol
    {
        string ParentTypeName { get; set; }    
    }
}