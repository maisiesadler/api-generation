using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Generatable;

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
            yield return new SchemaDefinition(name, openApiComponentSchema);
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
                yield return new SchemaDefinition(name, content.Schema);
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
                        yield return new SchemaDefinition(name, content.Schema);
                    }
                }
            }
        }
    }

    private static bool SchemaIsLocalModel(OpenApiSchema schema)
        => schema != null && schema.Reference == null
            && (schema.Type == "object" || schema.Type == "array")
            && schema.Properties?.Any() == true;
}
