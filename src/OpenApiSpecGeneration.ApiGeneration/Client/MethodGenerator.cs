using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.ApiGeneration.Client
{
    internal class MethodGenerator
    {
        internal static BlockSyntax CreateMethodBody(
            OperationType operationType,
            TypeSyntax? returnType)
        {
            var statements = new List<StatementSyntax>();
            statements.Add(CreateHttpRequestMessage(operationType));
            statements.Add(CallHttpClient());
            return SyntaxFactory.Block(statements);
        }


        // var request = new HttpRequestMessage();
        private static StatementSyntax CreateHttpRequestMessage(OperationType operationType)
        {
            var objectCreationExpression = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("HttpRequestMessage"))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName("Method"),
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("HttpMethod"),
                                        SyntaxFactory.IdentifierName(operationType.ToString()))),
                                SyntaxFactory.Token(SyntaxKind.CommaToken)
                            }
                        )
                    )
                );

            var variableDeclarator = SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("request"))
               .WithInitializer(
                   SyntaxFactory.EqualsValueClause(objectCreationExpression)
               );

            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var"),
                    SyntaxFactory.SingletonSeparatedList(variableDeclarator)
                )
            );
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

            var argumentList = SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request"))
                )
            );

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
    }
}
