using ApiGuard.Models;

namespace ApiGuard.Exceptions
{
    public class AttributeMismatchException : EndpointException
    {
        internal AttributeMismatchException(MyAttribute attribute) : base($"The {attribute} attribute has changed for {attribute.Parent}")
        {
        }
    }
}
