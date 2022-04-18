using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.Client;
using OpenApiSpecGeneration.Controller;
using OpenApiSpecGeneration.Interactor;
using OpenApiSpecGeneration.Model;

namespace OpenApiSpecGeneration
{
    public class ApiGenerator
    {
        public static IEnumerable<ClassDeclarationSyntax> GenerateControllers(OpenApiDocument document)
            => ControllerGenerator.GenerateControllers(document);

        public static IEnumerable<RecordDeclarationSyntax> GenerateModels(OpenApiDocument document)
            => ModelGenerator.GenerateModels(document);

        public static IEnumerable<InterfaceDeclarationSyntax> GenerateInteractors(OpenApiDocument document)
            => InteractorGenerator.GenerateInteractors(document);

        public static IEnumerable<ClassDeclarationSyntax> GenerateClients(OpenApiDocument document)
            => ClientGenerator.Generate(document);
    }
}
