using System;
using System.IO;
using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.Domain.Strategies;

namespace ApiGuard
{
    public static class ApiAssert
    {
        public static void HasNotChanged(params Type[] types)
        {
            foreach (var type in types)
            {
                HasNotChanged(type);
            }
        }

        private static void HasNotChanged(Type type)
        {
            var projectResolver = new ProjectResolver();

            var typeLoader = new ReflectionTypeLoader();
            var projectInfo = projectResolver.GetProjectInfo(type);

            var api = typeLoader.LoadApi(type);

            Directory.CreateDirectory(projectInfo.TestFolderPath);
            if (!projectResolver.ApiFileExists(projectInfo, type))
            {
                projectResolver.WriteApiToFile(projectInfo, type, api);
                return;
            }

            var existingApi = projectResolver.ReadApiFromFile(projectInfo, type);
            
            var compareStrategy = new BestGuessEndpointMatchingStrategy();
            var comparer = new ApiComparer(compareStrategy);
            comparer.Compare(existingApi, api);

            // Nice to have:
            // Fields / Events / Delegates
            // Supporting constructor arguments on attributes
            // Multiple public types per file
            // custom struct as type
            // Re-generate the output file if there are changes to the API that aren't breaking
        }
    }
}
