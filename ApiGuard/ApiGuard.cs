using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;

namespace ApiGuard
{
    public static class Assert
    {
        public static async Task HasNotChanged(Type type)
        {
            // Get the path to the project
            var assemblyPath = type.Assembly.Location;
            var projectName = type.Assembly.GetName().Name;
            var bin = Path.DirectorySeparatorChar + "bin";
            var testProjectPath = assemblyPath.Substring(0, assemblyPath.IndexOf(bin, StringComparison.InvariantCultureIgnoreCase));
            var solutionPath = testProjectPath.Substring(0, testProjectPath.LastIndexOf(Path.DirectorySeparatorChar));
            var apiProjectPath = Path.Combine(solutionPath, projectName, $"{projectName}.csproj");

            // Load the project into Roslyn workspace
            var analyzerManager = new AnalyzerManager();
            var adHocProject = analyzerManager.GetProject(apiProjectPath);
            var workspace = adHocProject.GetWorkspace();
            var project = workspace.CurrentSolution.Projects.Single(x => x.Name == projectName);

            // Build the project
            var compilation = await project.GetCompilationAsync();

            // Get semantic model for the type passed in
            var syntaxTree = compilation.GetSymbolsWithName(x => x == type.Name);

            // For each endpoint create an entry (Endpoint)
            // Each Endpoint contains the name of the method, return type and its arguments as well as targeted attributes 
            // A SignaturePart consists of a SignatureType (enum), ISignatureValue (string/Complex object) and Attributes
            // If the ISignatureValue is a complex object, we repeat the process for that object but also include public properties
            // Once this entire tree is parsed, see if there is an existing tree document in the repository
            // If there isn't, create one and mark as success
            // If there is, load the existing document into memory
            // Loop over the existing document elements
            // For each endpoint in there, find the corresponding endpoint in the new tree
            // Compare the basic endpoint data (name, return type, number of arguments)
            // If all are correct, compare the argument types in the order they were originally
            // If the type is a complex object, recurse into it and compare properties / methods
            // If at any time a mismatch is found, throw an exception
            // The exception to this is when we see a [BETA] or [OBSOLETE] attribute on it
            // Otherwise, return success


            // In a separate project, provide a [BETA] attribute and corresponding Roslyn analyzer so it shows a warning

            // Out of scope:
            // Protected modifier
            // Fields / Events / Delegates
            // Named arguments
            // Automatic pass if there is a major version increase+++++++++++
        }
    }
}
