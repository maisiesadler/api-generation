namespace OpenApiSpecGeneration.Generatable;

internal static class SchemaDefinitionExtensions
{
    internal static IEnumerable<PropertyDefinition> GetProperties(this SchemaDefinition schemaDefinition)
    {
        if (schemaDefinition.schema.Properties == null) yield break;

        foreach (var (propertyName, openApiProperty) in schemaDefinition.schema.Properties)
        {
            var type = openApiProperty.Type;
            if (type == null)
                type = openApiProperty.Reference.Id;

            if (type == null)
                throw new InvalidOperationException($"{propertyName} missing property type");

            yield return new PropertyDefinition(propertyName, type, openApiProperty);
        }
    }
}
