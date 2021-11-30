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
                    var returnType = GetReturnTypeSyntax(openApiMethod.responses);
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(
                            returnType,
                            SyntaxFactory.Identifier("Execute"))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    var methods = new[] { methodDeclaration };

                    var interfaceName = $"I{CsharpNamingExtensions.FirstLetterToUpper(method)}{CsharpNamingExtensions.PathToClassName(apiPath)}Interactor";

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

        private static TypeSyntax GetReturnTypeSyntax(
            IReadOnlyDictionary<string, OpenApiResponse> responses)
        {
            if (TryGetFirstReturnTypeComponentName(responses, out var componentName))
            {
                var arg = SyntaxFactory.ParseTypeName(componentName);
                return SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(arg))
                );
            }

            return SyntaxFactory.ParseTypeName("Task");
        }

        private static bool TryGetFirstReturnTypeComponentName(
            IReadOnlyDictionary<string, OpenApiResponse> responses,
            [NotNullWhen(true)] out string? returnType)
        {
            returnType = null;
            if (!responses.Any()) return false;
            var response = responses.First();
            if (!response.Value.content.Any()) return false;
            var content = response.Value.content.First();
            if (!content.Value.schema.items.TryGetValue("$ref", out var component)) return false;

            returnType = component.Split("/").Last();
            return true;
        }
    }
}
