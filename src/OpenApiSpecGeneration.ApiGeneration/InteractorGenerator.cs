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
            foreach (var (name, openApiPath) in spec.paths)
            {
                foreach (var (method, openApiMethod) in openApiPath)
                {
                    if (!TryGetReturnType(openApiMethod.responses, out var returnType))
                    {
                        // todo: warn?
                        continue;
                    }
                    var methods = new List<MemberDeclarationSyntax>();
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(
                            returnType,
                            SyntaxFactory.Identifier("Execute"))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    methods.Add(methodDeclaration);

                    var interfaceName = $"I{CsharpNamingExtensions.FirstLetterToUpper(method)}{CsharpNamingExtensions.PathToClassName(name)}Interactor";

                    var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration(
                        attributeLists: default,
                        modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                        keyword: SyntaxFactory.Token(SyntaxKind.InterfaceKeyword),
                        identifier: SyntaxFactory.Identifier(interfaceName),
                        typeParameterList: default,
                        baseList: null,
                        constraintClauses: default,
                        openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                        members: SyntaxFactory.List<MemberDeclarationSyntax>(
                            methods.ToArray()
                        ),
                        closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
                        semicolonToken: default);

                    members.Add(interfaceDeclaration);
                }
            }

            return members;
        }

        private static bool TryGetReturnType(
            IReadOnlyDictionary<string, OpenApiResponse> responses,
            [NotNullWhen(true)] out TypeSyntax? returnType)
        {
            returnType = null;
            if (!responses.Any()) return false;
            var response = responses.First();
            if (!response.Value.content.Any()) return false;
            var content = response.Value.content.First();
            var component = content.Value.schema.items["$ref"];
            var componentName = component.Split("/").Last();
            var arg = SyntaxFactory.ParseTypeName(componentName);
            returnType = SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"),
                SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(arg))
            );
            return true;
        }

        private static AttributeSyntax JsonPropertyNameAttributeSyntax(string propertyName)
        {
            var quotedPropertyName = $"\"{propertyName}\"";
            var attributeArgument = SyntaxFactory.AttributeArgument(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Token(SyntaxFactory.TriviaList(),
                        SyntaxKind.StringLiteralToken,
                        quotedPropertyName,
                        quotedPropertyName,
                        SyntaxFactory.TriviaList()
                    )
                )
            );

            return SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName("JsonPropertyName"),
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SingletonSeparatedList<AttributeArgumentSyntax>(attributeArgument)
                )
            );
        }
    }
}
