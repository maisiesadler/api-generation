using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.Generatable
{
    internal record ArgumentDefinition(AttributeSyntax attributeSyntax, TypeSyntax parameterTypeSyntax, string name);
}
