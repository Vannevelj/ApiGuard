using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public class EndpointMismatchException : EndpointException
    {
        internal EndpointMismatchException(MyMethod endpoint, MyMethod expectedEndpoint, string api) 
            : base(endpoint, $"The API has changed. Endpoint {expectedEndpoint} on {api} is now defined as {endpoint}")
        {
        }
    }
}
