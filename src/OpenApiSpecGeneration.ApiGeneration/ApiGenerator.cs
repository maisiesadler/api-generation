using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Controller;
using OpenApiSpecGeneration.Definition;
using OpenApiSpecGeneration.Interactor;
using OpenApiSpecGeneration.Model;

namespace OpenApiSpecGeneration
{
    public class ApiGenerator
    {
        public static IEnumerable<ClassDeclarationSyntax> GenerateControllers(Definition.Definition definition)
            => ControllerGenerator.GenerateControllers(definition);

        public static IEnumerable<RecordDeclarationSyntax> GenerateModels(Definition.Definition definition)
            => ModelGenerator.GenerateModels(definition);

        public static IEnumerable<InterfaceDeclarationSyntax> GenerateInteractors(Definition.Definition definition)
            => InteractorGenerator.GenerateInteractors(definition);

        public static IEnumerable<ClassDeclarationSyntax> GenerateControllers(OpenApiDocument document)
        {
            var definition = DefinitionGenerator.GenerateDefinition(document);
            return GenerateControllers(definition);
        }

        public static IEnumerable<RecordDeclarationSyntax> GenerateModels(OpenApiDocument document)
        {
            var definition = DefinitionGenerator.GenerateDefinition(document);
            return GenerateModels(definition);
        }

        public static IEnumerable<InterfaceDeclarationSyntax> GenerateInteractors(OpenApiDocument document)
        {
            var definition = DefinitionGenerator.GenerateDefinition(document);
            return GenerateInteractors(definition);
        }
    }
}
