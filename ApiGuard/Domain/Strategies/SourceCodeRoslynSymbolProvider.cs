using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ApiGuard.Domain.Strategies.Interfaces;
using ApiGuard.Exceptions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiGuard.Domain.Strategies
{
    internal class SourceCodeRoslynSymbolProvider : IRoslynSymbolProvider
    {
        public async Task<INamedTypeSymbol> GetApiClassSymbol(object input)
        {
            var source = (string) input;

            var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);

            var sourceTree = CSharpSyntaxTree.ParseText(source);
            var root = await sourceTree.GetRootAsync();
            var compilation = CSharpCompilation.Create("SourceData")
                                               .AddReferences(
                                                   MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                                   MetadataReference.CreateFromFile(typeof(DataMemberAttribute).Assembly.Location),
                                                   MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll"))
                                               .AddSyntaxTrees(sourceTree);

            var errors = compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Any())
            {
                var firstError = errors.First();
                throw new CompilationException(firstError.GetMessage(CultureInfo.CurrentCulture));
            }
            
            var model = compilation.GetSemanticModel(sourceTree);
            var myTypeSyntax = root.DescendantNodes().OfType<TypeDeclarationSyntax>().First();
            var apiSymbol = model.GetDeclaredSymbol(myTypeSyntax);
            return apiSymbol;
        }
    }
}
