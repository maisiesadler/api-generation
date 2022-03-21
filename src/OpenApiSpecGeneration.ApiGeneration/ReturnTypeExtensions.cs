using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    internal class ReturnTypeExtensions
    {
        internal static TypeSyntax GetReturnTypeSyntaxWrapped(IReadOnlyDictionary<string, OpenApiResponse> responses)
        {
            if (TryGetFirstReturnTypeSyntax(responses, out var typeSyntax))
            {
                return SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(typeSyntax)
                    )
                );
            }

            return SyntaxFactory.ParseTypeName("Task");
        }

        internal static bool HasReturnType(IReadOnlyDictionary<string, OpenApiResponse>? responses)
            => responses != null && TryGetFirstReturnTypeSyntax(responses, out var _);

        internal static bool TryGetFirstReturnTypeSyntax(
            IReadOnlyDictionary<string, OpenApiResponse> responses,
            [NotNullWhen(true)] out TypeSyntax? typeSyntax)
        {
            typeSyntax = null;
            return TryGetFirstReturnTypeSchema(responses, out var componentSchema)
                && TryConvertComponent(componentSchema, out typeSyntax);
        }

        private static bool TryGetFirstReturnTypeSchema(
            IReadOnlyDictionary<string, OpenApiResponse> responses,
            [NotNullWhen(true)] out OpenApiContentSchema? responseSchema)
        {
            responseSchema = null;
            if (!responses.Any()) return false;
            var response = responses.First();
            if (!response.Value.content.Any()) return false;
            var content = response.Value.content.First();
            responseSchema = content.Value.schema;

            return true;
        }

        private static bool TryConvertComponent(
            OpenApiContentSchema responseSchema,
            [NotNullWhen(true)] out TypeSyntax? typeSyntax)
        {
            if (!TryGetRef(responseSchema, out var component))
            {
                typeSyntax = null;
                return false;
            }

            var returnType = component.Split("/").Last();

            if (responseSchema.type == "array")
            {
                typeSyntax = SyntaxFactory.ArrayType(SyntaxFactory.ParseTypeName(returnType))
                    .WithRankSpecifiers(
                        SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                           SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression()))));

                return true;
            }

            typeSyntax = SyntaxFactory.ParseTypeName(returnType);
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
