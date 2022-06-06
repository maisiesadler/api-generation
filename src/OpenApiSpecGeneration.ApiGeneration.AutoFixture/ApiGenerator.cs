using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.AutoFixture.Implementation;
using OpenApiSpecGeneration.Definition;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture
{
    public class ApiGenerator
    {
        public static IEnumerable<ClassDeclarationSyntax> GenerateImplementations(Definition.Definition definition)
            => ImplementationGenerator.Generate(definition);

        public static IEnumerable<ClassDeclarationSyntax> GenerateImplementations(OpenApiDocument document)
        {
            var definition = DefinitionGenerator.GenerateDefinition(document);
            return GenerateImplementations(definition);
        }
    }
}
