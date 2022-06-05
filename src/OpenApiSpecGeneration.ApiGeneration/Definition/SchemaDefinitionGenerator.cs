using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Definition;

internal class SchemaDefinitionGenerator
{
    internal static IEnumerable<SchemaDefinition> Execute(OpenApiDocument document)
    {
        foreach (var (pathName, openApiPathItem) in document.Paths)
        {
            foreach (var (operationType, operation) in openApiPathItem.Operations)
            {
                foreach (var requestSchema in GetRequestSchemas(pathName, operationType, operation))
                    yield return requestSchema;

                foreach (var requestSchema in GetResponseSchemas(pathName, operationType, operation))
                    yield return requestSchema;
            }
        }

        foreach (var (name, openApiComponentSchema) in document.Components.Schemas)
        {
            foreach (var def in ToSchemaDefinition(name, openApiComponentSchema))
                yield return def;
        }
    }

    private static IEnumerable<SchemaDefinition> GetRequestSchemas(string pathName, OperationType operationType, OpenApiOperation operation)
    {
        if (operation.RequestBody == null) yield break;

        foreach (var (contentType, content) in operation.RequestBody.Content)
        {
            if (SchemaIsLocalModel(content.Schema))
            {
                var name = CsharpNamingExtensions.PathEtcToClassName(
                    new[] { pathName, operationType.ToString(), contentType, "Request" });
                foreach (var def in ToSchemaDefinition(name, content.Schema))
                    yield return def;
            }
        }
    }

    private static IEnumerable<SchemaDefinition> GetResponseSchemas(string pathName, OperationType operationType, OpenApiOperation operation)
    {
        if (operation.Responses == null) yield break;

        foreach (var (responseName, response) in operation.Responses)
        {
            if (response?.Content != null)
            {
                foreach (var (contentType, content) in response.Content)
                {
                    if (SchemaIsLocalModel(content.Schema))
                    {
                        var name = CsharpNamingExtensions.PathEtcToClassName(
                            new[] { pathName, operationType.ToString(), responseName, contentType, "Response" });
                        foreach (var def in ToSchemaDefinition(name, content.Schema))
                            yield return def;
                    }
                }
            }
        }
    }

    private static IEnumerable<SchemaDefinition> ToSchemaDefinition(string name, OpenApiSchema openApiSchema)
    {
        var properties = PropertyDefinitionGenerator.Execute(name, openApiSchema.Properties).ToArray();
        yield return new SchemaDefinition(name, properties);

        foreach (var definition in GetSubTypes(name, properties))
        {
            yield return definition;
        }
    }

    private static IEnumerable<SchemaDefinition> GetSubTypes(
        string schemaDefinitionName,
        PropertyDefinition[] propertyDefinitions)
    {
        foreach (var propertyDefinition in propertyDefinitions)
        {
            if (propertyDefinition.createObjectSubType)
            {
                foreach (var def in ToSchemaDefinition(propertyDefinition.potentialSubtypeName, propertyDefinition.property))
                    yield return def;
            }

            if (propertyDefinition.createArraySubType)
            {
                foreach (var def in ToSchemaDefinition(propertyDefinition.potentialSubtypeName, propertyDefinition.property.Items))
                    yield return def;
            }
        }
    }

    private static bool SchemaIsLocalModel(OpenApiSchema schema)
        => schema != null && schema.Reference == null
            && (schema.Type == "object" || schema.Type == "array")
            && schema.Properties?.Any() == true;
}
