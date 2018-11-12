using System;

namespace ApiGuard.Exceptions
{
    public class ApiNotFoundException : Exception
    {
        internal ApiNotFoundException(string api) : base($"Unable to find the API for type {api}")
        {

        }
    }
}
