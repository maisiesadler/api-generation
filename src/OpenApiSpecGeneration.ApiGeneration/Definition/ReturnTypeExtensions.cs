using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Definition
{
    internal class ReturnTypeDefintionGenerator
    {
        internal static bool HasReturnType(OpenApiResponses? responses)
            => responses != null && GetReturnType(responses).hasReturnType;

        internal static ReturnType GetReturnType(OpenApiResponses? responses)
        {
            if (TryGetFirstReturnTypeSchema(responses, out var componentSchema))
                return ConvertComponent(componentSchema);

            return new ReturnType(false, false, string.Empty);
        }

        private static bool TryGetFirstReturnTypeSchema(
            OpenApiResponses? responses,
            [NotNullWhen(true)] out OpenApiSchema? responseSchema)
        {
            responseSchema = null;
            if (responses?.Any() != true) return false;
            var response = responses.First();
            if (!response.Value.Content.Any()) return false;
            var content = response.Value.Content.First();
            responseSchema = content.Value.Schema;

            return true;
        }

        private static ReturnType ConvertComponent(OpenApiSchema schema)
        {
            if (!TryGetRef(schema, out var component))
            {
                return new ReturnType(false, false, string.Empty);
            }

            var returnType = component.Split("/").Last();

            if (schema.Type == "array")
            {
                return new ReturnType(true, true, returnType);
            }

            return new ReturnType(true, false, returnType);
        }

        private static bool TryGetRef(OpenApiSchema? schema, [NotNullWhen(true)] out string? @ref)
        {
            if (!string.IsNullOrWhiteSpace(schema?.Reference?.Id))
            {
                @ref = schema.Reference.Id;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(schema?.Items?.Reference?.Id))
            {
                @ref = schema.Items.Reference.Id;
                return true;
            }

            @ref = null;
            return false;
        }
    }
}
