using System.Collections.Generic;
using System.Linq;

namespace ApiGuard.Models
{
    internal class Api
    {
        public string TypeName { get; set; }
        public List<Endpoint> Endpoints { get; set; } = new List<Endpoint>();

        public Endpoint GetMatchingEndpoint(Endpoint otherEndpoint)
        {
            var endpoint = Endpoints.SingleOrDefault(x =>
            {
                if (x.MethodName != otherEndpoint.MethodName)
                {
                    return false;
                }

                if (x.Parameters.Count != otherEndpoint.Parameters.Count)
                {
                    return false;
                }

                if (x.ReturnType != otherEndpoint.ReturnType)
                {
                    return false;
                }

                if (otherEndpoint.Parameters.Count == 0)
                {
                    return true;
                }

                for (var index = 0; index < x.Parameters.Count; index++)
                {
                    var param = x.Parameters[index];
                    var matchingParam = otherEndpoint.Parameters[index];

                    if (param == matchingParam)
                    {
                        return true;
                    }

                    return false;
                }

                return false;
            });

            return endpoint;
        }
    }
}
