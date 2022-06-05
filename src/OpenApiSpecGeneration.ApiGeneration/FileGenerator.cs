using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    public class FileGenerator
    {
        public static IEnumerable<WritableFile> GenerateControllers(string @namespace, Definition.Definition definition)
           => ApiGenerator.GenerateControllers(definition)
            .Select(c =>
            {
                var usings = SyntaxFactory.List<UsingDirectiveSyntax>(new[]{
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"Microsoft.AspNetCore.Mvc")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Interactors")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Models")),
               });
                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{@namespace}"))
                    .AddMembers(c);

                return new WritableFile($"/{c.Identifier.Value}Controller.cs", usings, ns);
            });

        public static IEnumerable<WritableFile> GenerateModels(string @namespace, Definition.Definition definition)
            => ApiGenerator.GenerateModels(definition)
                .Select(model =>
                {
                    var usings = SyntaxFactory.List<UsingDirectiveSyntax>(new[]{
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"System.Text.Json.Serialization")),
                    });
                    var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{@namespace}.Models"))
                        .AddMembers(model);

                    return new WritableFile($"/models/{model.Identifier.Value}.cs", usings, ns);
                });

        public static IEnumerable<WritableFile> GenerateInteractors(string @namespace, Definition.Definition definition)
            => ApiGenerator.GenerateInteractors(definition)
                .Select(interactor =>
                {
                    var usings = SyntaxFactory.List<UsingDirectiveSyntax>(new[]{
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Models")),
                    });
                    var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{@namespace}.Interactors"))
                        .AddMembers(interactor);

                    return new WritableFile($"/interactors/{interactor.Identifier.Value}.cs", usings, ns);
                });
    }

    public record WritableFile(
        string fileLocation,
        SyntaxList<UsingDirectiveSyntax>? usingDirectiveSyntax,
        NamespaceDeclarationSyntax namespaceDeclarationSyntax);
}
