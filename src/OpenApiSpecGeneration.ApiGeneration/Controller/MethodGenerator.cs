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
            var methodBody = CreateMethodBody(propertyName, openApiMethod?.parameters, hasReturnType);
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

        private static BlockSyntax CreateMethodBody(string propertyName, OpenApiMethodParameter[]? parameters, bool hasReturnType)
        {
            var memberAccessExpressionSyntax = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName($"_{propertyName}"),
                SyntaxFactory.Token(SyntaxKind.DotToken),
                SyntaxFactory.IdentifierName("Execute")
            );

            var argumentList = CreateArgumentList(parameters);

            var invocationExpressionSyntax = SyntaxFactory.InvocationExpression(
                memberAccessExpressionSyntax,
                argumentList
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
                var attribute = ParamAttribute(openApiMethodParameter.In, openApiMethodParameter.name);
                var attributeList = SyntaxFactory.List<AttributeListSyntax>(
                    new[]{ SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(attribute)
                    )}
                );
                var name = CsharpNamingExtensions.HeaderToParameter(openApiMethodParameter.name);
                var typeSyntax = CsharpTypeExtensions.ParseTypeSyntax(openApiMethodParameter.schema?.type);
                var parameter = SyntaxFactory.Parameter(
                        attributeList,
                        default,
                        typeSyntax,
                        SyntaxFactory.Identifier(name),
                        default
                    );

                parameters.Add(parameter);
            }

            return SyntaxFactory.SeparatedList<ParameterSyntax>(parameters);
        }

        private static ArgumentListSyntax CreateArgumentList(OpenApiMethodParameter[]? openApiMethodParameters)
        {
            if (openApiMethodParameters == null) return SyntaxFactory.ArgumentList();

            var arguments = new List<ArgumentSyntax>();

            foreach (var openApiMethodParameter in openApiMethodParameters)
            {
                var name = CsharpNamingExtensions.HeaderToParameter(openApiMethodParameter.name);
                var argument = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(name));

                arguments.Add(argument);
            }

            var argumentSyntaxList = SyntaxFactory.SeparatedList<ArgumentSyntax>(arguments);
            return SyntaxFactory.ArgumentList(argumentSyntaxList);
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

        private static AttributeSyntax ParamAttribute(string? parameterLocation, string? parameterName)
        {
            AttributeSyntax AsAttribute(string attributeName, string? name = null)
            {
                if (name == null)
                    return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));

                return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName))
                    .WithArgumentList(
                        SyntaxFactory.AttributeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<AttributeArgumentSyntax>(
                                SyntaxFactory
                                    .AttributeArgument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(parameterName ?? string.Empty)))
                                    .WithNameEquals(
                                        SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(name))
                                    )
                            )
                        )
                    );
            }

            return parameterLocation switch
            {
                "path" => AsAttribute("FromRoute"),
                "query" => AsAttribute("FromQuery"),
                "header" => AsAttribute("FromHeader", "Name"),
                _ => throw new InvalidOperationException($"Unknown parameter type '{parameterLocation}'"),
            };
        }
    }
}
