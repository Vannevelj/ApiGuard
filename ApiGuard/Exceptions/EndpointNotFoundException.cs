using System.Linq;
using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public sealed class EndpointNotFoundException : EndpointException
    {
        internal EndpointNotFoundException(Endpoint endpoint, string api) 
            : base(endpoint, $"The API has changed. Unable to find endpoint {endpoint} on the interface of API {api}")
        {
            
        }
    }
}
