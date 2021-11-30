using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    public class ApiGenerator
    {
        public static IList<ClassDeclarationSyntax> GenerateControllers(OpenApiSpec spec)
            => ControllerGenerator.GenerateControllers(spec);

        public static IList<RecordDeclarationSyntax> GenerateModels(OpenApiSpec spec)
            => ModelGenerator.GenerateModels(spec);

        public static IList<InterfaceDeclarationSyntax> GenerateInteractors(OpenApiSpec spec)
            => InteractorGenerator.GenerateInteractors(spec);
    }
}
