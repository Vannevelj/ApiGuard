using ApiGuard.Models;

namespace ApiGuard.Domain.Interfaces
{
    internal interface IApiComparer
    {
        void Compare(Api originalApi, Api newApi);
    }
}
