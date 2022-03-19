using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.Controller
{
    internal class MethodGenerator
    {
        internal static MethodDeclarationSyntax CreateMethod(
            string method, OpenApiMethod? openApiMethod, string propertyType, string propertyName)
        {
            var hasReturnType = ReturnTypeExtensions.HasReturnType(openApiMethod?.responses);
            var methodBody = CreateMethodBody(propertyName, hasReturnType);
            var parameterList = CreateParameterList(openApiMethod?.parameters);
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task<IActionResult>"), CsharpNamingExtensions.FirstLetterToUpper(method))
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .WithBody(methodBody)
                .WithParameterList(SyntaxFactory.ParameterList(
                    SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    parameterList,
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                ))
                .AddAttributeLists(GetMethodAttributeList(method));
        }

        private static BlockSyntax CreateMethodBody(string propertyName, bool hasReturnType)
        {
            var memberAccessExpressionSyntax = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName($"_{propertyName}"),
                SyntaxFactory.Token(SyntaxKind.DotToken),
                SyntaxFactory.IdentifierName("Execute")
            );

            var invocationExpressionSyntax = SyntaxFactory.InvocationExpression(
                memberAccessExpressionSyntax,
                SyntaxFactory.ArgumentList()
            );

            var awaitExpressionSyntax = SyntaxFactory.AwaitExpression(
                SyntaxFactory.Token(SyntaxKind.AwaitKeyword),
                invocationExpressionSyntax
            );

            return hasReturnType
                ? SaveAndReturnResult(awaitExpressionSyntax)
                : CallAndReturnOk(awaitExpressionSyntax);
        }

        private static SeparatedSyntaxList<ParameterSyntax> CreateParameterList(OpenApiMethodParameter[]? openApiMethodParameters)
        {
            if (openApiMethodParameters == null) return SyntaxFactory.SeparatedList<ParameterSyntax>();

            var parameters = new List<ParameterSyntax>();

            foreach (var openApiMethodParameter in openApiMethodParameters)
            {
                if (openApiMethodParameter.In != "path") throw new InvalidOperationException($"Unknown parameter type '{openApiMethodParameter.In}'");

                var attributeList = SyntaxFactory.List<AttributeListSyntax>(
                    new[]{ SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromRoute"))
                        )
                    )}
                );
                var typeSyntax = CsharpTypeExtensions.ParseTypeSyntax(openApiMethodParameter.schema?.type);
                var parameter = SyntaxFactory.Parameter(
                        attributeList,
                        default,
                        typeSyntax,
                        SyntaxFactory.Identifier(openApiMethodParameter.name ?? string.Empty),
                        default
                    );

                parameters.Add(parameter);
            }

            return SyntaxFactory.SeparatedList<ParameterSyntax>(parameters);
        }

        private static BlockSyntax CallAndReturnOk(AwaitExpressionSyntax awaitExpressionSyntax)
        {
            var expressionStatementSyntax = SyntaxFactory.ExpressionStatement(awaitExpressionSyntax);

            var returnInvocationExpressionSyntax = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("Ok"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    SyntaxFactory.SeparatedList<ArgumentSyntax>(),
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken))
            );

            // return Ok();
            var returnStatementSyntax = SyntaxFactory.ReturnStatement(
                SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                returnInvocationExpressionSyntax,
                SyntaxFactory.Token(SyntaxKind.SemicolonToken)
            );

            var statements = new List<StatementSyntax> { expressionStatementSyntax, returnStatementSyntax };

            return SyntaxFactory.Block(statements);
        }

        private static BlockSyntax SaveAndReturnResult(AwaitExpressionSyntax awaitExpressionSyntax)
        {
            var variableDeclarator = SyntaxFactory.VariableDeclarator(
                SyntaxFactory.Identifier("result"),
                default,
                SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.Token(SyntaxKind.EqualsToken),
                    awaitExpressionSyntax
                )
            );

            // var executeStatement = SyntaxFactory.ParseStatement($"var result = await _{propertyName}.Execute();");
            var executeStatement = SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var"),
                    SyntaxFactory.SingletonSeparatedList(variableDeclarator)
                )
            );

            var returnInvocationExpressionSyntax = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("Ok"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("result"))
                    ),
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken))
            );

            // return Ok(result);
            var returnStatementSyntax = SyntaxFactory.ReturnStatement(
                SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                returnInvocationExpressionSyntax,
                SyntaxFactory.Token(SyntaxKind.SemicolonToken)
            );

            var statements = new List<StatementSyntax> { executeStatement, returnStatementSyntax };

            return SyntaxFactory.Block(statements);
        }

        private static AttributeListSyntax[] GetMethodAttributeList(string method)
        {
            var attributeNmae = RouteAttributeName(method);
            return new[] {
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeNmae))
                    )
                ),
            };
        }

        private static string RouteAttributeName(string method)
        {
            return method switch
            {
                "get" => "HttpGet",
                "post" => "HttpPost",
                "put" => "HttpPut",
                "delete" => "HttpDelete",
                _ => throw new InvalidOperationException($"Unknown method '{method}'"),
            };
        }
    }
}
