using System;
using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public class DefinitionMismatchException : Exception
    {
        internal DefinitionMismatchException(SymbolMismatch mismatch) 
            : base($"A mismatch on the API was found. Expected {mismatch.Expected} but received {mismatch.Received}")
        {
            
        }
    }
}
