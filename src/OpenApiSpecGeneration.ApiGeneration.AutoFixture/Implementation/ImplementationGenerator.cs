using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Generatable;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture.Implementation
{
    internal class ImplementationGenerator
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
                    var returnType = ReturnTypeExtensions.GetReturnTypeSyntaxWrapped(operation.Responses);
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

                    var interfaceName = CsharpNamingExtensions.PathToInteractorType(apiPath, operationType);
                    var className = interfaceName.Substring(1);

                    var classDeclaration = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(className))
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(interfaceName)))
                        .AddMembers(new[] { CreateField() })
                        .AddMembers(new[] { methodDeclaration })
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                    yield return classDeclaration;
                }
            }
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

        private static MemberDeclarationSyntax CreateField()
        {
            var fixtureTypeIdentifier = SyntaxFactory.IdentifierName("Fixture");
            var variableDeclaration = SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("_fixture"))
                .WithInitializer(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.ObjectCreationExpression(fixtureTypeIdentifier)
                            .WithArgumentList(SyntaxFactory.ArgumentList())
                    )
                );

            return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(fixtureTypeIdentifier)
                    .AddVariables(variableDeclaration)
                )
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
        }
    }
}
