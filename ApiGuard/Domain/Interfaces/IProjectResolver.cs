using System;
using ApiGuard.Models;

namespace ApiGuard.Domain.Interfaces
{
    internal interface IProjectResolver
    {
        ProjectInfo GetProjectInfo(Type type);

        bool ApiFileExists(ProjectInfo projectInfo, Type type);

        void WriteApiToFile(ProjectInfo projectInfo, Type type, MyType api);
    }
}
