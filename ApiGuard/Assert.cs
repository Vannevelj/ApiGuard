﻿using System;
using System.Threading.Tasks;
using ApiGuard.Domain;
using ApiGuard.Exceptions;

namespace ApiGuard
{
    public static class Assert
    {
        public static async Task HasNotChanged(params Type[] types)
        {
            foreach (var type in types)
            {
                await HasNotChanged(type);
            }
        }

        public static async Task HasNotChanged(Type type)
        {
            var projectResolver = new ProjectResolver();
            var typeLoader = new RoslynTypeLoader(projectResolver);
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

            foreach (var endpoint in existingApi.Endpoints)
            {
                var correspondingEndpoint = api.GetMatchingEndpoint(endpoint);
                if (correspondingEndpoint == null)
                {
                    throw new EndpointNotFoundException(endpoint, api.TypeName);
                }
            }


            // The exception to this is when we see a [BETA] or [OBSOLETE] attribute on it

            // In a separate project, provide a [BETA] attribute and corresponding Roslyn analyzer so it shows a warning

            // Out of scope:
            // Protected modifier
            // Fields / Events / Delegates
            // Named arguments
            // Datamember attributes on complex objects
            // Automatic pass if there is a major version increase
        }
    }
}
