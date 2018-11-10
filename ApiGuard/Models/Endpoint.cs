using System.Collections.Generic;

namespace ApiGuard.Models
{
    internal class Endpoint
    {
        public string MethodName { get; set; }
        public MyType ReturnType { get; set; }
        public List<MyType> Attributes { get; set; } = new List<MyType>();
        public List<MyParameter> Parameters { get; } = new List<MyParameter>();
    }
}
