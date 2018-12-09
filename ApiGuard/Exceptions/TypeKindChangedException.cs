using System;
using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public class TypeKindChangedException : Exception
    {
        internal TypeKindChangedException(SymbolMismatch mismatch)
            : base($"A mismatch on the API was found. The type of {mismatch.Expected} was changed")
        {
        }
    }
}
