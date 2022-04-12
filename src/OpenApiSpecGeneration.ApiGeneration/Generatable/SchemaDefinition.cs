using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Generatable;

internal record SchemaDefinition(string name, OpenApiSchema schema);

internal record PropertyDefinition(string propertyName, string propertyType, OpenApiSchema property);
