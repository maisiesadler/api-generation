using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Model;

internal record SchemaDefinition(string name, OpenApiSchema schema);
