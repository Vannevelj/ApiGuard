using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public class ElementRemovedException : DefinitionMismatchException
    {
        internal ElementRemovedException(SymbolMismatch mismatch) 
            : base($"A mismatch on the API was found. The element {mismatch.Expected} was removed")
        {
        }
    }
}
