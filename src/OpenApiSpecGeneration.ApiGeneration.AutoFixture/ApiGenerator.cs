using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.ApiGeneration.AutoFixture.Implementation;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture
{
    public class ApiGenerator
    {
        public static IEnumerable<ClassDeclarationSyntax> GenerateImplementations(OpenApiSpec spec)
            => ImplementationGenerator.Generate(spec);
    }
}
