using System;
using ApiGuard.Models;
using Newtonsoft.Json;

namespace ApiGuard.Exceptions
{
    public class EndpointException : Exception
    {
        internal EndpointException(Endpoint endpoint, string message) : base(message)
        {
            Data.Add("Endpoint", JsonConvert.SerializeObject(endpoint));
        }
    }
}
