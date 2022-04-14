using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Model;

internal static class PropertyDefinitionGenerator
{
    internal static IEnumerable<PropertyDefinition> Execute(
        string schemaDefinitionName,
        IDictionary<string, OpenApiSchema> properties)
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
            var potentialSubtypeName = schemaDefinitionName + CsharpNamingExtensions.SnakeCaseToCamel(propertyName) + "SubType";

            yield return new PropertyDefinition(propertyName, type, openApiProperty, createArraySubType, createObjectSubType, potentialSubtypeName);
        }
    }

    private static bool ShouldCreateObjectSubType(string? propertyType, OpenApiSchema property)
        => propertyType == "object" && property.Reference == null && property.Properties?.Any() == true;

    private static bool ShouldCreateArraySubType(string? propertyType, OpenApiSchema property)
        => propertyType == "array" && !CsharpTypeExtensions.TryGetPredefinedTypeSyntax(property.Items?.Type, out _);
}
