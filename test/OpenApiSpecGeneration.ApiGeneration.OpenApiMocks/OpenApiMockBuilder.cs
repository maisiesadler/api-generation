using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;

public static class OpenApiMockBuilder
{
    public static OpenApiDocument BuildDocument()
        => new OpenApiDocument
        {
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents { Schemas = new Dictionary<string, OpenApiSchema>() },
        };

    public static OpenApiDocument WithPath(
        this OpenApiDocument openApiDocument,
        string path,
        OpenApiPathItem pathItem)
    {
        openApiDocument.Paths.Add(path, pathItem);
        return openApiDocument;
    }

    public static OpenApiDocument WithComponentSchema(
        this OpenApiDocument openApiDocument,
        string key,
        OpenApiSchema schema)
    {
        openApiDocument.Components.Schemas.Add(key, schema);
        return openApiDocument;
    }

    public static Microsoft.OpenApi.Models.OpenApiResponse BuildResponse(string description)
    {
        return new Microsoft.OpenApi.Models.OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>(),
        };
    }

    public static Microsoft.OpenApi.Models.OpenApiResponse AddContent(
        this Microsoft.OpenApi.Models.OpenApiResponse openApiResponse,
        string contentType,
        OpenApiSchema schema)
    {
        var openApiMediaType = new OpenApiMediaType
        {
            Schema = schema,
        };

        openApiResponse.Content.Add(contentType, openApiMediaType);
        return openApiResponse;
    }

    public static OpenApiPathItem BuildPathItem()
    {
        return new OpenApiPathItem
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>(),
        };
    }

    public static OpenApiPathItem WithOperation(
       this OpenApiPathItem pathItem,
       OperationType operationType,
       Action<OpenApiOperation>? setup = null)
    {
        var operation = new OpenApiOperation
        {
            Parameters = new List<OpenApiParameter>(),
        };

        if (setup != null) setup(operation);

        pathItem.Operations.Add(operationType, operation);
        return pathItem;
    }
}
