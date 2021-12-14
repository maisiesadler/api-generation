using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.Entities;
using OpenApiSpecGeneration.SdkGeneration;

namespace OpenApiSpecGeneration.ImplementationGeneration.Implementation
{
    internal class ImplementationGenerator
    {
        internal static IEnumerable<InterfaceDeclarationSyntax> GenerateImplementations(OpenApiSpec spec)
        {
            return Array.Empty<InterfaceDeclarationSyntax>();
            // foreach (var (apiPath, openApiPath) in spec.paths)
            // {
            //     foreach (var (method, openApiMethod) in openApiPath.GetMethods())
            //     {
            // var returnType = ReturnTypeExtensions.GetReturnTypeSyntax(openApiMethod.responses);
            // var methodDeclaration = SyntaxFactory.MethodDeclaration(
            //         returnType,
            //         SyntaxFactory.Identifier("Execute"))
            //     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            // var methods = new[] { methodDeclaration };

            // var interfaceName = CsharpNamingExtensions.PathToInteractorType(apiPath, method);

            // var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration(
            //     attributeLists: default,
            //     modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
            //     keyword: SyntaxFactory.Token(SyntaxKind.InterfaceKeyword),
            //     identifier: SyntaxFactory.Identifier(interfaceName),
            //     typeParameterList: default,
            //     baseList: null,
            //     constraintClauses: default,
            //     openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
            //     members: SyntaxFactory.List<MemberDeclarationSyntax>(methods),
            //     closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
            //     semicolonToken: default);

            // yield return interfaceDeclaration;
            //     }
            // }
        }
    }
}
