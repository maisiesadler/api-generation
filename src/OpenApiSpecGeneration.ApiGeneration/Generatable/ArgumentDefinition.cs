using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.Generatable
{
    public record ArgumentDefinition(AttributeSyntax attributeSyntax, TypeSyntax parameterTypeSyntax, string name);
}
