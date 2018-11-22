using System.Linq;
using System.Threading.Tasks;
using ApiGuard.Domain.Strategies.Interfaces;
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

            var sourceTree = CSharpSyntaxTree.ParseText(source);
            var root = await sourceTree.GetRootAsync();
            var compilation = CSharpCompilation.Create("Source data")
                                               .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                                               .AddSyntaxTrees(sourceTree);
            var model = compilation.GetSemanticModel(sourceTree);
            var myTypeSyntax = root.DescendantNodes().OfType<TypeDeclarationSyntax>().First();
            var apiSymbol = model.GetDeclaredSymbol(myTypeSyntax);
            return apiSymbol;
        }
    }
}
