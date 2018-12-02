using System;

namespace ApiGuard.Exceptions
{
    public class CompilationException : Exception
    {
        public CompilationException(string message) : base($"Unable to verify API due to an unresolved compilation error: {message}")
        {
            
        }
    }
}
