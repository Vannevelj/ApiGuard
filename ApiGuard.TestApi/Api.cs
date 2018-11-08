using System;
using System.Text;

namespace ApiGuard.TestApi
{
    public class Api
    {
        public void Method()
        {

        }

        public void MethodWithSimpleArgs(int a, string b, DateTime c)
        {

        }

        public void MethodWithComplexArgs(StringBuilder a, Args b)
        {

        }
    }

    public class Args
    {
        public int Id { get; set; }
        public Opts Options { get; set; }

        public void DoSomething(int a, Opts b)
        {

        }
    }

    public class Opts
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
