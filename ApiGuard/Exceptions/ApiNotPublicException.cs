using System;

namespace ApiGuard.Exceptions
{
    public class ApiNotPublicException : Exception
    {
        internal ApiNotPublicException(string type) : base($"The type {type} has to be public")
        {

        }
    }
}
