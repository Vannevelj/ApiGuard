using System;
using System.IO;

namespace ApiGuard.Models
{
    internal class ProjectInfo
    {
        private const string TestFolderName = "generated_api";

        public string SolutionPath { get; set; }
        public string ProjectName { get; set; }
        public string ProjectFilePath { get; set; }
        public string TestProjectPath { get; set; }
        public string TestFolderPath => Path.Combine(TestProjectPath, TestFolderName);       

        public string GetApiFilePath(Type type)
        {
            return Path.Combine(TestProjectPath, TestFolderName, $"api_{type.Name}.json");
        }
    }
}
