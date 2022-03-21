using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture.Implementation
{
    internal class MethodGenerator
    {
        internal static BlockSyntax CreateMethodBody(TypeSyntax? typeToGenerate, TypeSyntax returnType)
        {
            if (typeToGenerate == null)
                return SyntaxFactory.Block();

            var memberAccessExpressionSyntax = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("_fixture"),
                SyntaxFactory.Token(SyntaxKind.DotToken),
                SyntaxFactory.GenericName(SyntaxFactory.Identifier("Create"))
                    .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(typeToGenerate))
                    )
            );

            var invocationExpressionSyntax = SyntaxFactory.InvocationExpression(memberAccessExpressionSyntax);

            var returnStatementSyntax = SyntaxFactory.ReturnStatement(
                SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                invocationExpressionSyntax,
                SyntaxFactory.Token(SyntaxKind.SemicolonToken)
            );

            var statements = new List<StatementSyntax> { returnStatementSyntax };

            return SyntaxFactory.Block(statements);
        }
    }
}
