using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.ApiGeneration.Client
{
    internal class MethodGenerator
    {
        internal static BlockSyntax CreateMethodBody(TypeSyntax? returnType)
        {
            var statements = new List<StatementSyntax>();
            statements.Add(CallHttpClient());
            return SyntaxFactory.Block(statements);
        }

        // var response = await _httpClient.SendAsync();
        private static StatementSyntax CallHttpClient()
        {
            var memberAccessExpressionSyntax = SyntaxFactory.MemberAccessExpression(
               SyntaxKind.SimpleMemberAccessExpression,
               SyntaxFactory.IdentifierName($"_httpClient"),
               SyntaxFactory.Token(SyntaxKind.DotToken),
               SyntaxFactory.IdentifierName("SendAsync")
           );

            var argumentList = CreateHttpClientArgumentList();

            var invocationExpressionSyntax = SyntaxFactory.InvocationExpression(
                memberAccessExpressionSyntax,
                argumentList
            );

            var awaitExpressionSyntax = SyntaxFactory.AwaitExpression(
                SyntaxFactory.Token(SyntaxKind.AwaitKeyword),
                invocationExpressionSyntax
            );

            var variableDeclarator = SyntaxFactory.VariableDeclarator("response")
                .WithInitializer(
                        SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.Token(SyntaxKind.EqualsToken),
                        awaitExpressionSyntax
                    )
                );

            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var"),
                    SyntaxFactory.SingletonSeparatedList(variableDeclarator)
                )
            );
        }

        private static ArgumentListSyntax CreateHttpClientArgumentList()
        {
            var arguments = new List<ArgumentSyntax>
            {
                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request"))
            };

            var argumentSyntaxList = SyntaxFactory.SeparatedList<ArgumentSyntax>(arguments);
            return SyntaxFactory.ArgumentList(argumentSyntaxList);
        }
    }
}
