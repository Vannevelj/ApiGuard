using System;
using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public class MemberAddedToInterfaceException : Exception
    {
        internal MemberAddedToInterfaceException(SymbolMismatch mismatch)
            : base($"A mismatch on the API was found. A member was added to the interface of {mismatch.Expected}")
        {
        }
    }
}
