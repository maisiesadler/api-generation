using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Controller;
using OpenApiSpecGeneration.Interactor;
using OpenApiSpecGeneration.Model;

namespace OpenApiSpecGeneration
{
    public class FileGenerator
    {
        public static IEnumerable<WritableFile> GenerateControllers(string @namespace, OpenApiDocument document)
           => ControllerGenerator.GenerateControllers(document)
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

        public static IEnumerable<WritableFile> GenerateModels(string @namespace, OpenApiDocument document)
            => ModelGenerator.GenerateModels(document)
                .Select(model =>
                {
                    var usings = SyntaxFactory.List<UsingDirectiveSyntax>(new[]{
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"System.Text.Json.Serialization")),
                    });
                    var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{@namespace}.Models"))
                        .AddMembers(model);

                    return new WritableFile($"/models/{model.Identifier.Value}.cs", usings, ns);
                });

        public static IEnumerable<WritableFile> GenerateInteractors(string @namespace, OpenApiDocument document)
            => InteractorGenerator.GenerateInteractors(document)
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
