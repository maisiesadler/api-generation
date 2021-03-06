using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture
{
    public class FileGenerator
    {
        public static IEnumerable<WritableFile> GenerateImplementation(string @namespace, OpenApiDocument document)
           => ApiGenerator.GenerateImplementations(document)
            .Select(c =>
            {
                var usings = SyntaxFactory.List<UsingDirectiveSyntax>(new[]{
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"AutoFixture")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Interactors")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Models")),
               });
                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{@namespace}.GeneratedImplementations"))
                    .AddMembers(c);

                return new WritableFile($"/implementations/{c.Identifier.Value}.cs", usings, ns);
            });
    }
}
