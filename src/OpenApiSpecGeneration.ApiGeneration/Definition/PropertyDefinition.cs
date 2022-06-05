using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Definition;

public record PropertyDefinition(
    string propertyName,
    string propertyType,
    OpenApiSchema property,
    bool createArraySubType,
    bool createObjectSubType,
    string potentialSubtypeName);
