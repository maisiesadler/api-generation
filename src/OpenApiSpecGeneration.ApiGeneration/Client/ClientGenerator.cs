using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Generatable;

namespace OpenApiSpecGeneration.ApiGeneration.Client
{
    internal class ClientGenerator
    {
        internal static IEnumerable<ClassDeclarationSyntax> Generate(OpenApiDocument document)
        {
            foreach (var (apiPath, openApiPath) in document.Paths)
            {
                foreach (var (operationType, operation) in openApiPath.Operations)
                {
                    var argumentDefinitions = ArgumentDefinitionGenerator.Create(apiPath, operationType, operation.RequestBody, operation.Parameters).ToArray();
                    var parameters = CreateParameterList(argumentDefinitions);
                    var typeToGenerate = ReturnTypeExtensions.TryGetFirstReturnTypeSyntax(operation.Responses, out var rt)
                        ? rt : null;
                    var returnType = ReturnTypeExtensions.GetReturnTypeSyntaxAsTask(operation.Responses);
                    var methodBody = MethodGenerator.CreateMethodBody(typeToGenerate, returnType);
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(
                            returnType,
                            SyntaxFactory.Identifier("Execute"))
                        .AddModifiers(
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.AsyncKeyword)
                        )
                        .WithParameterList(parameters)
                        .WithBody(methodBody);

                    var clientClassName = CsharpNamingExtensions.PathToClientType(apiPath, operationType);

                    var classDeclaration = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(clientClassName))
                        .AddMembers(CreateConstructor(clientClassName))
                        .AddMembers(CreateHttpClientField())
                        .AddMembers(methodDeclaration)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                    yield return classDeclaration;
                }
            }
        }

        private static ConstructorDeclarationSyntax CreateConstructor(string className)
        {
            var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("httpClient"))
                .WithType(SyntaxFactory.ParseTypeName("HttpClient"));

            var ctor = SyntaxFactory.ConstructorDeclaration(className)
                .AddParameterListParameters(parameter)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(CreateAssignment("httpClient")));

            return ctor;
        }

        private static StatementSyntax CreateAssignment(string propertyName)
        {
            var fieldIdentifier = SyntaxFactory.IdentifierName($"_{propertyName}");
            var propertyIdentifier = SyntaxFactory.IdentifierName(propertyName);
            var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, propertyIdentifier);
            return SyntaxFactory.ExpressionStatement(assignment);
        }

        private static MemberDeclarationSyntax CreateHttpClientField()
        {
            var fieldTokens = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
            );

            var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("HttpClient"))
                .AddVariables(SyntaxFactory.VariableDeclarator("_httpClient"));

            return SyntaxFactory.FieldDeclaration(variableDeclaration)
                .WithModifiers(fieldTokens);
        }

        private static ParameterListSyntax CreateParameterList(IList<ArgumentDefinition> openApiParameters)
        {
            if (openApiParameters == null) return SyntaxFactory.ParameterList();

            var parameters = new List<ParameterSyntax>();

            foreach (var openApiMethodParameter in openApiParameters)
            {
                var attributeList = SyntaxFactory.List<AttributeListSyntax>();
                var name = CsharpNamingExtensions.HeaderToParameter(openApiMethodParameter.name);
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(name))
                    .WithType(openApiMethodParameter.parameterTypeSyntax);

                parameters.Add(parameter);
            }

            return SyntaxFactory.ParameterList(
                SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                SyntaxFactory.SeparatedList<ParameterSyntax>(parameters),
                SyntaxFactory.Token(SyntaxKind.CloseParenToken)
            );
        }
    }
}
