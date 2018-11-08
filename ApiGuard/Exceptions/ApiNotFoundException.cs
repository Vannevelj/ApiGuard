using System;
using System.Collections.Generic;
using System.Text;

namespace ApiGuard.Exceptions
{
    internal class ApiNotFoundException : Exception
    {
        public ApiNotFoundException(string api) : base($"Unable to find the API for type {api}")
        {

        }
    }
}
