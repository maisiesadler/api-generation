using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.Controller;
using OpenApiSpecGeneration.Model;

namespace OpenApiSpecGeneration
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
