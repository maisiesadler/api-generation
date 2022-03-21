using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture
{
    public class FileGenerator
    {
        public static IEnumerable<WritableFile> GenerateImplementation(string @namespace, OpenApiSpec spec)
           => ApiGenerator.GenerateImplementations(spec)
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
