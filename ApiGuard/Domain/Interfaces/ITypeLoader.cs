using System;
using System.Threading.Tasks;
using ApiGuard.Models;

namespace ApiGuard.Domain.Interfaces
{
    internal interface ITypeLoader
    {
        Task<Api> LoadApi(object input);
    }
}
