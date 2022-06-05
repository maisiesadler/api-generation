using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.Definition;

namespace OpenApiSpecGeneration
{
    internal class ReturnTypeExtensions_2
    {
        internal static TypeSyntax GetReturnTypeSyntaxAsTask(ReturnType returnType)
        {
            if (TryGetReturnTypeSyntax(returnType, out var typeSyntax))
            {
                return SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(typeSyntax)
                    )
                );
            }

            return SyntaxFactory.ParseTypeName("Task");
        }

        internal static bool HasReturnType(ReturnType returnType)
            => TryGetReturnTypeSyntax(returnType, out var _);

        private static bool TryGetReturnTypeSyntax(
            ReturnType returnType,
            [NotNullWhen(true)] out TypeSyntax? typeSyntax)
        {
            if (!returnType.hasReturnType)
            {
                typeSyntax = null;
                return false;
            }

            if (returnType.isArray)
            {
                typeSyntax = SyntaxFactory.ArrayType(SyntaxFactory.ParseTypeName(returnType.value))
                    .WithRankSpecifiers(
                        SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                           SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression()))));

                return true;
            }

            typeSyntax = SyntaxFactory.ParseTypeName(returnType.value);
            return true;
        }
    }
}
