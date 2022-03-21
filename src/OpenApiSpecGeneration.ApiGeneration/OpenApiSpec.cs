using System.Text.Json.Serialization;

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
        public OpenApiMethodParameter[]? parameters { get; init; }
    }

    public record OpenApiMethodParameterSchema(string? type, int? minimum);
    public record OpenApiMethodParameter
    {
        [JsonPropertyName("in")] public string? In { get; init; }
        public string? name { get; init; }
        public bool? required { get; init; }
        public string? description { get; init; }
        public OpenApiMethodParameterSchema? schema { get; init; }
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
    public record OpenApiComponentProperty(string? type, OpenApiComponentPropertyType? items, string? format, bool? nullable)
    {
        [JsonPropertyName("$ref")] public string? Ref { get; init; }
    }

    public record OpenApiComponentPropertyType(IReadOnlyDictionary<string, OpenApiComponentProperty>? properties, string? type);
}
