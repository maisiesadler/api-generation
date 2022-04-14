using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Model;

internal record PropertyDefinition(
    string propertyName,
    string propertyType,
    OpenApiSchema property,
    bool createArraySubType,
    bool createObjectSubType,
    string potentialSubtypeName);
