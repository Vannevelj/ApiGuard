using System.Collections.Generic;

namespace ApiGuard.Models
{
    internal class MyParameter
    {
        public MyType Type { get; set; }
        public int Ordinal { get; set; }
    }

    internal interface IElement { }

    internal class MyMethod : IElement
    {
        public MyType ReturnType { get; set; }
        public string Name { get; set; }
        public List<MyParameter> Parameters { get; set; } = new List<MyParameter>();
    }

    internal class MyProperty : IElement
    {
        public MyType Type { get; set; }
        public string Name { get; set; }
    }

    internal class MyType
    {
        public string Typename { get; set; }
        public List<IElement> NestedElements { get; } = new List<IElement>();
    }
}
