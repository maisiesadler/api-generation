using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    internal class InteractorGenerator
    {
        internal static IList<InterfaceDeclarationSyntax> GenerateInteractors(OpenApiSpec spec)
        {
            var members = new List<InterfaceDeclarationSyntax>();
            foreach (var (apiPath, openApiPath) in spec.paths)
            {
                foreach (var (method, openApiMethod) in openApiPath)
                {
                    var returnType = ReturnTypeExtensions.GetReturnTypeSyntax(openApiMethod.responses);
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(
                            returnType,
                            SyntaxFactory.Identifier("Execute"))
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

                    members.Add(interfaceDeclaration);
                }
            }

            return members;
        }
    }
}
