using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.Entities;
using OpenApiSpecGeneration.ImplementationGeneration.Implementation;

namespace OpenApiSpecGeneration.SdkGeneration
{
    public class FileGenerator
    {
        public static IEnumerable<WritableFile> GenerateControllers(string @namespace, OpenApiSpec spec)
          => ApiGeneration.FileGenerator.GenerateControllers(@namespace, spec);

        public static IEnumerable<WritableFile> GenerateModels(string @namespace, OpenApiSpec spec)
            => ApiGeneration.FileGenerator.GenerateModels(@namespace, spec);

        public static IEnumerable<WritableFile> GenerateInteractors(string @namespace, OpenApiSpec spec)
            => ApiGeneration.FileGenerator.GenerateInteractors(@namespace, spec);

        public static IEnumerable<WritableFile> GenerateImplementations(string @namespace, OpenApiSpec spec)
            => ImplementationGenerator.GenerateImplementations(spec)
                .Select(interactor =>
                {
                    var usings = SyntaxFactory.List<UsingDirectiveSyntax>(new[]{
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Interactors")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Models")),
                    });
                    var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{@namespace}.Implementations"))
                        .AddMembers(interactor);

                    return new WritableFile($"/implementations/{interactor.Identifier.Value}.cs", usings, ns);
                });
    }
}
