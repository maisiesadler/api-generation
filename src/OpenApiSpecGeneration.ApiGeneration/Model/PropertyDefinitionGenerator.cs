using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Model;

internal static class PropertyDefinitionGenerator
{
    internal static IEnumerable<PropertyDefinition> Execute(IDictionary<string, OpenApiSchema> properties)
    {
        if (properties == null) yield break;

        foreach (var (propertyName, openApiProperty) in properties)
        {
            var type = openApiProperty.Type;
            if (type == null)
                type = openApiProperty.Reference.Id;

            if (type == null)
                throw new InvalidOperationException($"{propertyName} missing property type");

            var createArraySubType = ShouldCreateArraySubType(type, openApiProperty);
            var createObjectSubType = ShouldCreateObjectSubType(type, openApiProperty);

            yield return new PropertyDefinition(propertyName, type, openApiProperty, createArraySubType, createObjectSubType);
        }
    }

    private static bool TryGetPredefinedTypeSyntax(string? propertyType, [NotNullWhen(true)] out PredefinedTypeSyntax? predefinedTypeSyntax)
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

        private static bool ShouldCreateObjectSubType(string? propertyType, OpenApiSchema property)
            => propertyType == "object" && property.Reference == null && property.Properties?.Any() == true;

        private static bool ShouldCreateArraySubType(string? propertyType, OpenApiSchema property)
            => propertyType == "array" && !TryGetPredefinedTypeSyntax(property.Items?.Type, out _);
}
