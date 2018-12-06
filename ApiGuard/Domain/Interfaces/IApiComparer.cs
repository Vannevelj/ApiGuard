using ApiGuard.Models;

namespace ApiGuard.Domain.Interfaces
{
    internal interface IApiComparer
    {
        void Compare(MyType originalApi, MyType newApi);
    }
}
