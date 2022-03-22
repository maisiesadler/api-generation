using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Controller
{
    internal class MethodGenerator
    {
        internal static MethodDeclarationSyntax CreateMethod(
            OperationType operationType, OpenApiOperation? operation, string propertyType, string propertyName)
        {
            var hasReturnType = ReturnTypeExtensions.HasReturnType(operation?.Responses);
            var methodBody = CreateMethodBody(propertyName, operation?.Parameters, hasReturnType);
            var parameterList = CreateParameterList(operation?.Parameters);
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task<IActionResult>"), operationType.ToString())
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .WithBody(methodBody)
                .WithParameterList(SyntaxFactory.ParameterList(
                    SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    parameterList,
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                ))
                .AddAttributeLists(GetMethodAttributeList(operationType));
        }

        private static BlockSyntax CreateMethodBody(string propertyName, IList<OpenApiParameter>? parameters, bool hasReturnType)
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

        private static SeparatedSyntaxList<ParameterSyntax> CreateParameterList(IList<OpenApiParameter>? openApiMethodParameters)
        {
            if (openApiMethodParameters == null) return SyntaxFactory.SeparatedList<ParameterSyntax>();

            var parameters = new List<ParameterSyntax>();

            foreach (var openApiMethodParameter in openApiMethodParameters)
            {
                var attribute = ParamAttribute(openApiMethodParameter.In, openApiMethodParameter.Name);
                var attributeList = SyntaxFactory.List<AttributeListSyntax>(
                    new[]{ SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(attribute)
                    )}
                );
                var name = CsharpNamingExtensions.HeaderToParameter(openApiMethodParameter.Name);
                var typeSyntax = CsharpTypeExtensions.ParseTypeSyntax(openApiMethodParameter.Schema?.Type);
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

        private static ArgumentListSyntax CreateArgumentList(IList<OpenApiParameter>? openApiMethodParameters)
        {
            if (openApiMethodParameters == null) return SyntaxFactory.ArgumentList();

            var arguments = new List<ArgumentSyntax>();

            foreach (var openApiMethodParameter in openApiMethodParameters)
            {
                var name = CsharpNamingExtensions.HeaderToParameter(openApiMethodParameter.Name);
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

        private static AttributeListSyntax[] GetMethodAttributeList(OperationType operationType)
        {
            var attributeNmae = RouteAttributeName(operationType);
            return new[] {
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeNmae))
                    )
                ),
            };
        }

        private static string RouteAttributeName(OperationType operationType)
        {
            return operationType switch
            {
                OperationType.Get => "HttpGet",
                OperationType.Post => "HttpPost",
                OperationType.Put => "HttpPut",
                OperationType.Delete => "HttpDelete",
                _ => throw new InvalidOperationException($"Unsupported operation type '{operationType}'"),
            };
        }

        private static AttributeSyntax ParamAttribute(ParameterLocation? parameterLocation, string? parameterName)
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
                ParameterLocation.Path => AsAttribute("FromRoute"),
                ParameterLocation.Query => AsAttribute("FromQuery"),
                ParameterLocation.Header => AsAttribute("FromHeader", "Name"),
                _ => throw new InvalidOperationException($"Unknown parameter type '{parameterLocation}'"),
            };
        }
    }
}
