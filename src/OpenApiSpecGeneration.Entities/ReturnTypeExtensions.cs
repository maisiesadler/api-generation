using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.Entities
{
    public class ReturnTypeExtensions
    {
        public static TypeSyntax GetReturnTypeSyntax(IReadOnlyDictionary<string, OpenApiResponse> responses)
        {
            if (TryGetFirstReturnTypeComponentName(responses, out var componentName))
            {
                var arg = SyntaxFactory.ParseTypeName(componentName);
                return SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(arg))
                );
            }

            return SyntaxFactory.ParseTypeName("Task");
        }

        public static bool HasReturnType(IReadOnlyDictionary<string, OpenApiResponse>? responses)
            => responses != null && TryGetFirstReturnTypeComponentName(responses, out var _);

        private static bool TryGetFirstReturnTypeComponentName(
            IReadOnlyDictionary<string, OpenApiResponse> responses,
            [NotNullWhen(true)] out string? returnType)
        {
            returnType = null;
            if (!responses.Any()) return false;
            var response = responses.First();
            if (!response.Value.content.Any()) return false;
            var content = response.Value.content.First();
            var component = string.Empty;

            if (!TryGetRef(content.Value?.schema, out component)) return false;

            returnType = component.Split("/").Last();
            return true;
        }

        private static bool TryGetRef(OpenApiContentSchema? schema, [NotNullWhen(true)] out string? @ref)
        {
            if (!string.IsNullOrWhiteSpace(schema?.Ref))
            {
                @ref = schema.Ref;
                return true;
            }

            if (schema?.items?.TryGetValue("$ref", out var component) == true)
            {
                @ref = component;
                return true;
            }

            @ref = null;
            return false;
        }
    }
}
