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

    public static OpenApiRequestBody BuildRequestBody(string description)
    {
        return new OpenApiRequestBody
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>(),
        };
    }

    public static OpenApiResponse BuildResponse(string description)
    {
        return new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>(),
        };
    }

    public static OpenApiRequestBody AddContent(
        this OpenApiRequestBody openApiRequestBody,
        string contentType,
        OpenApiSchema schema)
    {
        openApiRequestBody.Content.AddContent(contentType, schema);
        return openApiRequestBody;
    }

    public static OpenApiResponse AddContent(
        this OpenApiResponse openApiResponse,
        string contentType,
        OpenApiSchema schema)
    {
        openApiResponse.Content.AddContent(contentType, schema);
        return openApiResponse;
    }

    private static IDictionary<string, OpenApiMediaType> AddContent(
        this IDictionary<string, OpenApiMediaType> content,
        string contentType,
        OpenApiSchema schema)
    {
        var openApiMediaType = new OpenApiMediaType
        {
            Schema = schema,
        };

        content.Add(contentType, openApiMediaType);
        return content;
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
