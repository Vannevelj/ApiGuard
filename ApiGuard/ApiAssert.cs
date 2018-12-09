using System;
using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.Domain.Strategies;

namespace ApiGuard
{
    public static class ApiAssert
    {
        public static async Task HasNotChanged(params Type[] types)
        {
            foreach (var type in types)
            {
                await HasNotChanged(type);
            }
        }

        private static async Task HasNotChanged(Type type)
        {
            var projectResolver = new ProjectResolver();
            var symbolProvider = new FileSystemRoslynSymbolProvider(projectResolver);
            var typeLoader = new RoslynTypeLoader(symbolProvider);
            var projectInfo = projectResolver.GetProjectInfo(type);

            var api = await typeLoader.LoadApi(type);

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
            // adding methods to an interface/abstract type
            // Re-generate the output file if there are changes to the API that aren't breaking
        }
    }
}
