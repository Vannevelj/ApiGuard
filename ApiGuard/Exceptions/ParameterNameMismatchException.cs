using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public class ParameterNameMismatchException : DefinitionMismatchException
    {
        internal ParameterNameMismatchException(MyParameter oldParameter, MyParameter newParameter)
            : base($"A mismatch on the API was found. The parameter on {oldParameter.GetShorthandParent()} was renamed from {oldParameter.Name} to {newParameter.Name}")
        {
        }
    }
}
