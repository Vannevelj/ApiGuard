using System;
using System.IO;

namespace ApiGuard.Models
{
    internal class ProjectInfo
    {
        public string SolutionPath { get; set; }
        public string ProjectName { get; set; }
        public string ProjectFilePath { get; set; }

        public string GetApiFilePath(Type type)
        {
            return Path.Combine(SolutionPath, ProjectName, $"api_{type.Name}.json");
        }
    }
}
