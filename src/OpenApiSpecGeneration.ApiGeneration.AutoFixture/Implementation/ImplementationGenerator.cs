using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture.Implementation
{
    internal class ImplementationGenerator
    {
        internal static IEnumerable<ClassDeclarationSyntax> Generate(OpenApiSpec spec)
        {
            foreach (var (apiPath, openApiPath) in spec.paths)
            {
                foreach (var (method, openApiMethod) in openApiPath.GetMethods())
                {
                    var parameters = CreateParameterList(openApiMethod.parameters);
                    var typeToGenerate = ReturnTypeExtensions.TryGetFirstReturnTypeSyntax(openApiMethod.responses, out var rt)
                        ? rt : null;
                    var returnType = ReturnTypeExtensions.GetReturnTypeSyntaxWrapped(openApiMethod.responses);
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

                    var interfaceName = CsharpNamingExtensions.PathToInteractorType(apiPath, method);
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

        private static ParameterListSyntax CreateParameterList(OpenApiMethodParameter[]? openApiMethodParameters)
        {
            if (openApiMethodParameters == null) return SyntaxFactory.ParameterList();

            var parameters = new List<ParameterSyntax>();

            foreach (var openApiMethodParameter in openApiMethodParameters)
            {
                var attributeList = SyntaxFactory.List<AttributeListSyntax>();
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
