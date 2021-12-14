using System.Text.Json.Serialization;

namespace OpenApiSpecGeneration.Entities
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

    public record OpenApiContentSchema
    {
        public OpenApiContentSchema() { }
        public OpenApiContentSchema(string type, IReadOnlyDictionary<string, string>? items)
            => (this.type, this.items) = (type, items);

        public string? type { get; init; }
        public IReadOnlyDictionary<string, string>? items { get; init; }
        [JsonPropertyName("$ref")] public string? Ref { get; init; }
    }

    public record OpenApiComponent(IReadOnlyDictionary<string, OpenApiComponentSchema> schemas);
    public record OpenApiComponentSchema(string type, IReadOnlyDictionary<string, OpenApiComponentProperty> properties);
    public record OpenApiComponentProperty(string type, OpenApiComponentPropertyType? items, string? format, bool? nullable);
    public record OpenApiComponentPropertyType(IReadOnlyDictionary<string, OpenApiComponentProperty> properties);
}
