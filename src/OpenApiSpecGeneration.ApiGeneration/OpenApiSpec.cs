namespace OpenApiSpecGeneration
{
    public record OpenApiSpec(IReadOnlyDictionary<string, OpenApiPath> paths, OpenApiComponent components);
    public class OpenApiPath
    {
        public OpenApiMethod? get { get; set; }
        public OpenApiMethod? put { get; set; }
        public OpenApiMethod? post { get; set; }
        public OpenApiMethod? delete { get; set; }
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
    public record OpenApiComponentProperty(string type, OpenApiComponentPropertyType? items, string? format, bool? nullable);
    public record OpenApiComponentPropertyType(IReadOnlyDictionary<string, OpenApiComponentPropertyTypeItem> properties);
    public record OpenApiComponentPropertyTypeItem(string type, string? description);
}
