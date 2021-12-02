using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    public class FileGenerator
    {
        public static IEnumerable<WritableFile> GenerateControllers(string @namespace, OpenApiSpec spec)
           => ControllerGenerator.GenerateControllers(spec)
            .Select(c =>
            {
                var usings = SyntaxFactory.List<UsingDirectiveSyntax>(new[]{
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"Microsoft.AspNetCore.Mvc")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Interactors")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Models")),
               });
                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{@namespace}"))
                    .AddMembers(c);

                return new WritableFile($"{c.Identifier.Value}Controller.cs", usings, ns);
            });

        // public static IList<RecordDeclarationSyntax> GenerateModels(OpenApiSpec spec)
        //     => ModelGenerator.GenerateModels(spec);

        // public static IList<InterfaceDeclarationSyntax> GenerateInteractors(OpenApiSpec spec)
        //     => InteractorGenerator.GenerateInteractors(spec);
    }
}
