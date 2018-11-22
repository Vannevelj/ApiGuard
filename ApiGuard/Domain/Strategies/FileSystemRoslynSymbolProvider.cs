using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiGuard.Domain.Interfaces;
using ApiGuard.Domain.Strategies.Interfaces;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;

namespace ApiGuard.Domain.Strategies
{
    internal class FileSystemRoslynSymbolProvider : IRoslynSymbolProvider
    {
        private readonly IProjectResolver _projectResolver;

        public FileSystemRoslynSymbolProvider(IProjectResolver projectResolver)
        {
            _projectResolver = projectResolver;
        }

        public async Task<INamedTypeSymbol> GetApiClassSymbol(object input)
        {
            var type = (Type) input;

            var projectInfo = _projectResolver.GetProjectInfo(type);
            var apiProjectPath = projectInfo.ProjectFilePath;

            // Load the project into Roslyn workspace
            var analyzerManager = new AnalyzerManager();
            var adHocProject = analyzerManager.GetProject(apiProjectPath);
            var workspace = adHocProject.GetWorkspace();
            var project = workspace.CurrentSolution.Projects.Single(x => x.Name == projectInfo.ProjectName);

            // Build the project
            var compilation = await project.GetCompilationAsync();

            // Get symbol for the type passed in
            var apiSymbol = compilation.GetSymbolsWithName(x => x == type.Name).OfType<INamedTypeSymbol>().Single();
            return apiSymbol;
        }
    }
}
