using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.AutoFixture.Implementation;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture
{
    public class ApiGenerator
    {
        public static IEnumerable<ClassDeclarationSyntax> GenerateImplementations(OpenApiDocument document)
            => ImplementationGenerator.Generate(document);
    }
}
