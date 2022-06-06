namespace OpenApiSpecGeneration.Definition;

public record PropertyDefinition(
    string propertyName,
    string propertyType,
    bool createArraySubType,
    bool createObjectSubType,
    string potentialSubtypeName,
    string? subTypeName);
