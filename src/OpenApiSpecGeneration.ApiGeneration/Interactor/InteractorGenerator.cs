using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.Interactor
{
    internal class InteractorGenerator
    {
        internal static IEnumerable<InterfaceDeclarationSyntax> GenerateInteractors(OpenApiSpec spec)
        {
            foreach (var (apiPath, openApiPath) in spec.paths)
            {
                foreach (var (method, openApiMethod) in openApiPath.GetMethods())
                {
                    var parameters = CreateParameterList(openApiMethod.parameters);
                    var returnType = ReturnTypeExtensions.GetReturnTypeSyntax(openApiMethod.responses);
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(
                            returnType,
                            SyntaxFactory.Identifier("Execute"))
                        .WithParameterList(parameters)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    var methods = new[] { methodDeclaration };

                    var interfaceName = CsharpNamingExtensions.PathToInteractorType(apiPath, method);

                    var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration(
                        attributeLists: default,
                        modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                        keyword: SyntaxFactory.Token(SyntaxKind.InterfaceKeyword),
                        identifier: SyntaxFactory.Identifier(interfaceName),
                        typeParameterList: default,
                        baseList: null,
                        constraintClauses: default,
                        openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                        members: SyntaxFactory.List<MemberDeclarationSyntax>(methods),
                        closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
                        semicolonToken: default);

                    yield return interfaceDeclaration;
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
    }
}
