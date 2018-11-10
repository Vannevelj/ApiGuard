using System;
using System.IO;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Models;
using Newtonsoft.Json;

namespace ApiGuard.Domain
{
    internal class ProjectResolver : IProjectResolver
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public ProjectResolver()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public ProjectInfo GetProjectInfo(Type type)
        {
            var assemblyPath = type.Assembly.Location;
            var projectName = type.Assembly.GetName().Name;
            var bin = Path.DirectorySeparatorChar + "bin";
            var testProjectPath = assemblyPath.Substring(0, assemblyPath.IndexOf(bin, StringComparison.InvariantCultureIgnoreCase));
            var solutionPath = testProjectPath.Substring(0, testProjectPath.LastIndexOf(Path.DirectorySeparatorChar));
            var apiProjectPath = Path.Combine(solutionPath, projectName, $"{projectName}.csproj");

            return new ProjectInfo
            {
                ProjectName = projectName,
                SolutionPath = solutionPath,
                ProjectFilePath = apiProjectPath
            };
        }

        public bool ApiFileExists(ProjectInfo projectInfo, Type type)
        {
            var documentPath = projectInfo.GetApiFilePath(type);
            return File.Exists(documentPath);
        }

        public void WriteApiToFile(ProjectInfo projectInfo, Type type, Api api)
        {
            File.WriteAllText(projectInfo.GetApiFilePath(type), JsonConvert.SerializeObject(api, Formatting.Indented, _serializerSettings));
        }

        public Api ReadApiFromFile(ProjectInfo projectInfo, Type type)
        {
            try
            {
                var existingApiJson = File.ReadAllText(projectInfo.GetApiFilePath(type));
                return JsonConvert.DeserializeObject<Api>(existingApiJson, _serializerSettings);
            }
            catch (Exception e)
            {
                throw new FileNotFoundException("Unable to find or open the API file", e);
            }
        }
    }
}
