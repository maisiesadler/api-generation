using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.Model
{
    internal class ModelGenerator
    {
        internal static IEnumerable<RecordDeclarationSyntax> GenerateModels(OpenApiSpec spec)
        {
            return GenerateRecords(spec).ToArray();
        }

        private static IEnumerable<RecordDeclarationSyntax> GenerateRecords(OpenApiSpec spec)
        {
            foreach (var (name, openApiComponentSchema) in spec.components.schemas)
            {
                var (record, subtypes) = TryGenerateRecord(name, openApiComponentSchema.properties);
                yield return record;

                foreach (var (subtypename, subtype) in subtypes)
                {
                    yield return TryGenerateSubtypeRecord(subtypename, subtype);
                }
            }
        }

        private static RecordDeclarationSyntax TryGenerateSubtypeRecord(
            string subTypeName,
            OpenApiComponentPropertyType subType)
        {
            var properties = new List<MemberDeclarationSyntax>();

            foreach (var (propertyName, openApiProperty) in subType.properties)
            {
                var attributes = SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        JsonPropertyNameAttributeSyntax(propertyName)
                    )
                );

                var (typeSyntax, _) = ParseTypeSyntax(propertyName, openApiProperty.type);

                var property = SyntaxFactory.PropertyDeclaration(
                        typeSyntax,
                        SyntaxFactory.Identifier(CsharpNamingExtensions.FirstLetterToUpper(propertyName)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                    .AddAttributeLists(attributes);

                properties.Add(property);
            }

            var record = SyntaxFactory.RecordDeclaration(
                attributeLists: default,
                modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                keyword: SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                identifier: SyntaxFactory.Identifier(subTypeName),
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

            return record;
        }

        private static (RecordDeclarationSyntax, IList<(string, OpenApiComponentPropertyType)>) TryGenerateRecord(
            string name,
            IReadOnlyDictionary<string, OpenApiComponentProperty> openApiProperties)
        {
            var properties = new List<MemberDeclarationSyntax>();
            var subTypes = new List<(string, OpenApiComponentPropertyType)>();

            foreach (var (propertyName, openApiProperty) in openApiProperties)
            {
                var attributes = SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        JsonPropertyNameAttributeSyntax(propertyName)
                    )
                );

                var potentialSubtypeName = name + CsharpNamingExtensions.FirstLetterToUpper(propertyName) + "SubType";
                var (typeSyntax, createSubType) = ParseTypeSyntax(potentialSubtypeName, openApiProperty.type);

                var property = SyntaxFactory.PropertyDeclaration(
                        typeSyntax,
                        SyntaxFactory.Identifier(CsharpNamingExtensions.FirstLetterToUpper(propertyName)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                    .AddAttributeLists(attributes);

                properties.Add(property);

                if (createSubType && openApiProperty.items != null)
                {
                    subTypes.Add((potentialSubtypeName, openApiProperty.items));
                }
            }

            var record = SyntaxFactory.RecordDeclaration(
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

            return (record, subTypes);
        }

        private static (TypeSyntax typeSyntax, bool createSubType) ParseTypeSyntax(
            string potentialSubtypeName, string propertyType)
        {
            return propertyType switch
            {
                "integer" => (SyntaxFactory.ParseTypeName("int"), false),
                "string" => (SyntaxFactory.ParseTypeName("string"), false),
                "boolean" => (SyntaxFactory.ParseTypeName("bool"), false),
                "array" => (SyntaxFactory.ParseTypeName($"{potentialSubtypeName}[]"), true),
                _ => throw new InvalidOperationException($"Unknown openapi type '{propertyType}'"),
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
