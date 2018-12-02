using System;

namespace ApiGuard.Exceptions
{
    public class EndpointException : Exception
    {
        internal EndpointException(string message) : base(message)
        {
        }
    }
}
