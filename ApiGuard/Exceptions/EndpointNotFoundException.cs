using System.Linq;
using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public sealed class EndpointNotFoundException : EndpointException
    {
        internal EndpointNotFoundException(Endpoint endpoint, string api) : base(endpoint, $"The API has changed. Unable to find endpoint {endpoint.ReturnType.Typename} {endpoint.MethodName}({string.Join(", ", endpoint.Parameters.OrderBy(x => x.Ordinal).Select(x => x.Type.Typename))}) on the interface of API {api}")
        {
            
        }
    }
}
