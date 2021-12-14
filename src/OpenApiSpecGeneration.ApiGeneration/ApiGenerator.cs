using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.ApiGeneration.Controller;
using OpenApiSpecGeneration.Entities;
using OpenApiSpecGeneration.ApiGeneration.Interactor;
using OpenApiSpecGeneration.ApiGeneration.Model;

namespace OpenApiSpecGeneration.ApiGeneration
{
    public class ApiGenerator
    {
        public static IEnumerable<ClassDeclarationSyntax> GenerateControllers(OpenApiSpec spec)
            => ControllerGenerator.GenerateControllers(spec);

        public static IEnumerable<RecordDeclarationSyntax> GenerateModels(OpenApiSpec spec)
            => ModelGenerator.GenerateModels(spec);

        public static IEnumerable<InterfaceDeclarationSyntax> GenerateInteractors(OpenApiSpec spec)
            => InteractorGenerator.GenerateInteractors(spec);
    }
}
