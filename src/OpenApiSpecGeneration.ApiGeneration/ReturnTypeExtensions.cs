using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    internal class ReturnTypeExtensions
    {
        internal static TypeSyntax GetReturnTypeSyntax(IReadOnlyDictionary<string, OpenApiResponse> responses)
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

        internal static bool HasReturnType(IReadOnlyDictionary<string, OpenApiResponse> responses)
            => TryGetFirstReturnTypeComponentName(responses, out var _);

        private static bool TryGetFirstReturnTypeComponentName(
            IReadOnlyDictionary<string, OpenApiResponse> responses,
            [NotNullWhen(true)] out string? returnType)
        {
            returnType = null;
            if (!responses.Any()) return false;
            var response = responses.First();
            if (!response.Value.content.Any()) return false;
            var content = response.Value.content.First();
            if (!content.Value.schema.items.TryGetValue("$ref", out var component)) return false;

            returnType = component.Split("/").Last();
            return true;
        }
    }
}