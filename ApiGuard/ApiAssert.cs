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
            
            if (existingApi.TypeName != api.TypeName)
            {
                throw new ApiNotFoundException(existingApi.TypeName);
            }

            foreach (var endpointResult in existingApi.GetApiDifferences(api))
            {
                if (endpointResult.ExistingEndpoint == null)
                {
                    throw new EndpointNotFoundException(endpointResult.ExistingEndpoint, api.TypeName);
                }

                if (!endpointResult.IsExactMatch)
                {
                    var differentEndpointDefinition = endpointResult.SymbolsChanged.SingleOrDefault(x => x.Received.Equals(endpointResult.ExistingEndpoint));
                    if (differentEndpointDefinition != null)
                    {
                        throw new EndpointMismatchException(endpointResult.ReceivedEndpoint, endpointResult.ExistingEndpoint, api.TypeName);
                    }

                    var innerMostMismatch = endpointResult.SymbolsChanged.OrderByDescending(x => x.Received.Depth).First();
                    throw new DefinitionMismatchException(innerMostMismatch);
                }
            }

            // TODO: if there is a change in a type in the hierarchy of the method, use that in the error message
            // TODO: use the options
            // The exception to this is when we see a [BETA] or [OBSOLETE] attribute on it

            // In a separate project, provide a [BETA] attribute and corresponding Roslyn analyzer so it shows a warning

            // Nice to have:
            // Protected modifier
            // Fields / Events / Delegates
            // Named arguments
            // Datamember attributes on complex objects
            // Automatic pass if there is a major version increase
            // Attribute add/remove
            // Re-ordering members inside a type
            // adding member to interface/abstract class
        }
    }
}
