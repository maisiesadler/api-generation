using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration
{
    internal class ReturnTypeExtensions
    {
        internal static TypeSyntax GetReturnTypeSyntaxAsTask(OpenApiResponses? responses)
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

        internal static bool HasReturnType(OpenApiResponses? responses)
            => responses != null && TryGetFirstReturnTypeSyntax(responses, out var _);

        internal static bool TryGetFirstReturnTypeSyntax(
            OpenApiResponses? responses,
            [NotNullWhen(true)] out TypeSyntax? typeSyntax)
        {
            typeSyntax = null;
            return TryGetFirstReturnTypeSchema(responses, out var componentSchema)
                && TryConvertComponent(componentSchema, out typeSyntax);
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

        private static bool TryConvertComponent(
            OpenApiSchema schema,
            [NotNullWhen(true)] out TypeSyntax? typeSyntax)
        {
            if (!TryGetRef(schema, out var component))
            {
                typeSyntax = null;
                return false;
            }

            var returnType = component.Split("/").Last();

            if (schema.Type == "array")
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
