using System;
using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public class DefinitionMismatchException : Exception
    {
        internal DefinitionMismatchException(SymbolMismatch mismatch, bool withParentInfo = false) 
            : base($"A mismatch on the API was found. Expected {(withParentInfo ? mismatch.Expected.Parent : mismatch.Expected)} but received {(withParentInfo ? mismatch.Received.Parent : mismatch.Received)}")
        {
            
        }

        internal DefinitionMismatchException(string message) : base(message)
        {
            
        }
    }
}
