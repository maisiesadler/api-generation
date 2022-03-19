using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    internal static class CsharpTypeExtensions
    {
        internal static TypeSyntax ParseTypeSyntax(string? propertyType)
        {
            return propertyType switch
            {
                "integer" => SyntaxFactory.ParseTypeName("int"),
                "string" => SyntaxFactory.ParseTypeName("string"),
                "boolean" => SyntaxFactory.ParseTypeName("bool"),
                _ => throw new InvalidOperationException($"Unknown openapi type '{propertyType}'"),
            };
        }
    }
}
