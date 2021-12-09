using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.Model
{
    internal class ModelGenerator
    {
        internal static IEnumerable<RecordDeclarationSyntax> GenerateModels(OpenApiSpec spec)
        {
            var members = new List<RecordDeclarationSyntax>();
            foreach (var (name, openApiComponentSchema) in spec.components.schemas)
            {
                var properties = new List<MemberDeclarationSyntax>();

                foreach (var (propertyName, openApiProperty) in openApiComponentSchema.properties)
                {
                    var attributes = SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                            JsonPropertyNameAttributeSyntax(propertyName)
                        )
                    );

                    var property = SyntaxFactory.PropertyDeclaration(
                            ParseTypeSyntax(openApiProperty.type),
                            SyntaxFactory.Identifier(CsharpNamingExtensions.FirstLetterToUpper(propertyName)))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                        .AddAttributeLists(attributes);
                    properties.Add(property);
                }

                var recordDeclaration = SyntaxFactory.RecordDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                    keyword: SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                    identifier: SyntaxFactory.Identifier(name),
                    typeParameterList: default,
                    parameterList: default,
                    baseList: null,
                    constraintClauses: default,
                    openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                    members: SyntaxFactory.List<MemberDeclarationSyntax>(
                        properties.ToArray()
                    ),
                    closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
                    semicolonToken: default);

                members.Add(recordDeclaration);
            }

            return members;
        }

        private static TypeSyntax ParseTypeSyntax(string openApiType)
        {
            return openApiType switch
            {
                "integer" => SyntaxFactory.ParseTypeName("int"),
                "string" => SyntaxFactory.ParseTypeName("string"),
                "boolean" => SyntaxFactory.ParseTypeName("bool"),
                _ => throw new InvalidOperationException($"Unknown openapi type '{openApiType}'"),
            };
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
