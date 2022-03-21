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

                    var methods = new[] { methodDeclaration };

                    var interfaceName = CsharpNamingExtensions.PathToInteractorType(apiPath, method);
                    var className = interfaceName.Substring(1);

                    var classDeclaration = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(className))
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(interfaceName)))
                        .AddMembers(new[] { CreateField() })
                        .AddMembers(methods.ToArray())
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

            var list = SyntaxFactory.SeparatedList<ParameterSyntax>(parameters);
            return SyntaxFactory.ParameterList(
                SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                list,
                SyntaxFactory.Token(SyntaxKind.CloseParenToken)
            );
        }

        private static MemberDeclarationSyntax CreateField()
        {
            var fieldTokens = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
            );

            return SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName("Fixture"))
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier("_fixture"))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.IdentifierName("Fixture"))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList()))))))
                    .WithModifiers(fieldTokens);
        }
    }
}
