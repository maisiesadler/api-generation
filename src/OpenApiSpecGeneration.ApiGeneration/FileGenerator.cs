using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.ApiGeneration.Controller;
using OpenApiSpecGeneration.Entities;
using OpenApiSpecGeneration.ApiGeneration.Interactor;
using OpenApiSpecGeneration.ApiGeneration.Model;

namespace OpenApiSpecGeneration.ApiGeneration
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

        public static IEnumerable<WritableFile> GenerateModels(string @namespace, OpenApiSpec spec)
            => ModelGenerator.GenerateModels(spec)
                .Select(model =>
                {
                    var usings = SyntaxFactory.List<UsingDirectiveSyntax>(new[]{
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"System.Text.Json.Serialization")),
                    });
                    var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{@namespace}.Models"))
                        .AddMembers(model);

                    return new WritableFile($"/models/{model.Identifier.Value}.cs", usings, ns);
                });

        public static IEnumerable<WritableFile> GenerateInteractors(string @namespace, OpenApiSpec spec)
            => InteractorGenerator.GenerateInteractors(spec)
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
}
