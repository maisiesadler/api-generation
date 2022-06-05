using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Definition;

internal record PropertyDefinition(
    string propertyName,
    string propertyType,
    OpenApiSchema property,
    bool createArraySubType,
    bool createObjectSubType,
    string potentialSubtypeName);
