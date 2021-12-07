namespace OpenApiSpecGeneration
{
    public record OpenApiSpec(IReadOnlyDictionary<string, OpenApiPath> paths, OpenApiComponent components);
    public class OpenApiPath : Dictionary<string, OpenApiMethod>
    {
    }

    public record OpenApiMethod
    {
        public IReadOnlyCollection<string> tags { get; init; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, OpenApiResponse> responses { get; init; } = new Dictionary<string, OpenApiResponse>();
    }

    public record OpenApiResponse(string description, IReadOnlyDictionary<string, OpenApiContent> content);

    public record OpenApiContent(OpenApiContentSchema schema);

    public record OpenApiContentSchema(string type, IReadOnlyDictionary<string, string> items);

    public record OpenApiComponent(IReadOnlyDictionary<string, OpenApiComponentSchema> schemas);
    public record OpenApiComponentSchema(string type, IReadOnlyDictionary<string, OpenApiComponentProperty> properties);
    public record OpenApiComponentProperty(string type, string? format, bool? nullable);
}
