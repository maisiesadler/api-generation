using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Definition;
using OpenApiSpecGeneration.Generatable;

namespace OpenApiSpecGeneration.Controller
{
    internal class MethodGenerator
    {
        internal static MethodDeclarationSyntax CreateMethod(
            Operation operation,
            string propertyName)
        {
            var methodBody = CreateMethodBody(propertyName, operation.arguments, operation.hasReturnType);
            var parameterList = CreateParameterList(operation.arguments);
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task<IActionResult>"), operation.type.ToString())
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .WithBody(methodBody)
                .WithParameterList(SyntaxFactory.ParameterList(
                    SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    parameterList,
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                ))
                .AddAttributeLists(GetMethodAttributeList(operation.type));
        }

        private static BlockSyntax CreateMethodBody(string propertyName, ArgumentDefinition[] argumentDefinitions, bool hasReturnType)
        {
            var memberAccessExpressionSyntax = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName($"_{propertyName}"),
                SyntaxFactory.Token(SyntaxKind.DotToken),
                SyntaxFactory.IdentifierName("Execute")
            );

            var argumentList = CreateArgumentList(argumentDefinitions);

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

        private static ParameterSyntax CreateParameterWithAttribute(ArgumentDefinition argumentDefinition)
        {
            var attributeList = SyntaxFactory.List<AttributeListSyntax>(
                new[]{ SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(argumentDefinition.attributeSyntax)
                    )}
            );

            return SyntaxFactory.Parameter(SyntaxFactory.Identifier(argumentDefinition.name))
                .WithType(argumentDefinition.parameterTypeSyntax)
                .WithAttributeLists(attributeList);
        }

        private static ArgumentListSyntax CreateArgumentList(ArgumentDefinition[] argumentDefinitions)
        {
            if (argumentDefinitions.Length == 0) return SyntaxFactory.ArgumentList();

            var arguments = new List<ArgumentSyntax>();

            foreach (var argumentDefinition in argumentDefinitions)
            {
                var argument = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(argumentDefinition.name));

                arguments.Add(argument);
            }

            var argumentSyntaxList = SyntaxFactory.SeparatedList<ArgumentSyntax>(arguments);
            return SyntaxFactory.ArgumentList(argumentSyntaxList);
        }

        private static SeparatedSyntaxList<ParameterSyntax> CreateParameterList(ArgumentDefinition[] argumentDefinitions)
        {
            var parameters = new List<ParameterSyntax>();

            foreach (var argumentDefinition in argumentDefinitions)
            {
                var parameter = CreateParameterWithAttribute(argumentDefinition);
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
    }
}
