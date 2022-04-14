using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    internal static class CsharpTypeExtensions
    {
        internal static TypeSyntax ParseTypeSyntax(string? propertyType)
        {
            if (TryGetPredefinedTypeSyntax(propertyType, out var typeSyntax))
                return typeSyntax;

            throw new InvalidOperationException($"Unknown openapi type '{propertyType}'");
        }

        internal static bool TryGetPredefinedTypeSyntax(string? propertyType, [NotNullWhen(true)] out PredefinedTypeSyntax? predefinedTypeSyntax)
        {
            switch (propertyType)
            {
                case "integer":
                    {
                        predefinedTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
                        return true;
                    }
                case "string":
                    {
                        predefinedTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
                        return true;
                    }
                case "boolean":
                    {
                        predefinedTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));
                        return true;
                    }
                default:
                    {
                        predefinedTypeSyntax = null;
                        return false;
                    }
            };
        }
    }
}
