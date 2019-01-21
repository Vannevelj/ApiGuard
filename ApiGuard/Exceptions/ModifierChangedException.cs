using ApiGuard.Models;
using System;

namespace ApiGuard.Exceptions
{
    public class ModifierChangedException : Exception
    {
        internal ModifierChangedException(SymbolMismatch mismatch)
            : base($"A mismatch on the API was found. A modifier changed on {mismatch.Expected}")
        {
        }
    }
}
