using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.Domain.Strategies;
using ApiGuard.Exceptions;

namespace ApiGuard
{
    public static class ApiAssert
    {
        public static async Task HasNotChanged(ConfigurationOptions options, params Type[] types)
        {
            foreach (var type in types)
            {
                await HasNotChanged(options, type);
            }
        }

        public static async Task HasNotChanged(ConfigurationOptions options, Type type)
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

            // TODO: use the options
            // The exception to this is when we see a [BETA] or [OBSOLETE] attribute on it

            // In a separate project, provide a [BETA] attribute and corresponding Roslyn analyzer so it shows a warning

            // Nice to have:
            // Protected modifier
            // Fields / Events / Delegates
            // Parameter name
            // Automatic pass if there is a major version increase
            // Re-ordering members inside a type
            // adding member to interface/abstract class
            // Supporting constructor arguments on attributes
            // Multiple public types per file
            // custom struct as type
            // Change Fill to GetType in RoslynTypeLoader
            // Re-generate the output file if there are changes to the API that aren't breaking

            // New matching strategy:
            // 1. Look for same-type member (property/method/attribute) by name
            // 2. If found, compare them
            // 3. If not found, look for another member with a different name but same properties (return type/parameters)
            // 4. If found, compare them
            // 5. If not found, throw an ElementRemovedException
            // Code it in a way that there are 2 phases (resolving by name & resolving by signature)
        }
    }
}
