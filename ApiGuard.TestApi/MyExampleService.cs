using System;
using System.Text;
using System.Threading;

namespace ApiGuard.TestApi
{
    public class MyExampleService
    {
        public void Method()
        {

        }

        public void MethodWithSimpleArgs(string a, string b, DateTime c)
        {

        }

        public void MethodWithComplexArgs(StringBuilder a, Args b, CancellationToken token)
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