using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public sealed class EndpointNotFoundException : EndpointException
    {
        internal EndpointNotFoundException(MyMethod endpoint) : base($"The API has changed. Unable to find endpoint {endpoint}")
        {
            
        }
    }
}
