using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.Entities;

namespace OpenApiSpecGeneration.ApiGeneration.Model
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
                var (record, subtypes) = TryGenerateRecord(name, GetProperties(openApiComponentSchema.properties));
                yield return record;

                foreach (var (subtypename, subtype) in subtypes)
                {
                    var (subtypeRecord, heello) = TryGenerateRecord(subtypename, GetProperties(subtype));
                    yield return subtypeRecord;

                    foreach (var (_subtypename, _subtype) in heello)
                    {
                        var (_subtypeRecord, _) = TryGenerateRecord(_subtypename, GetProperties(_subtype));
                        yield return _subtypeRecord;
                    }
                }
            }
        }

        private static IEnumerable<(string propertyName, string propertyType, OpenApiComponentPropertyType? items)>
            GetProperties(IReadOnlyDictionary<string, OpenApiComponentProperty> openApiProperties)
        {
            foreach (var (propertyName, openApiProperty) in openApiProperties)
            {
                yield return (propertyName, openApiProperty.type, openApiProperty.items);
            }
        }

        private static IEnumerable<(string propertyName, string propertyType, OpenApiComponentPropertyType? items)>
            GetProperties(OpenApiComponentPropertyType openApiComponentPropertyType)
        {
            foreach (var (propertyName, openApiProperty) in openApiComponentPropertyType.properties)
            {
                yield return (propertyName, openApiProperty.type, openApiProperty.items);
            }
        }

        private static (RecordDeclarationSyntax, IList<(string, OpenApiComponentPropertyType)>) TryGenerateRecord(
            string name,
            IEnumerable<(string propertyName, string propertyType, OpenApiComponentPropertyType? items)> openApiProperties)
        {
            var properties = new List<MemberDeclarationSyntax>();
            var subTypes = new List<(string, OpenApiComponentPropertyType)>();

            foreach (var (propertyName, propertyType, items) in openApiProperties)
            {
                var attributes = SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        JsonPropertyNameAttributeSyntax(propertyName)
                    )
                );

                var potentialSubtypeName = name + CsharpNamingExtensions.SnakeCaseToCamel(propertyName) + "SubType";
                var (typeSyntax, createSubType) = ParseTypeSyntax(potentialSubtypeName, propertyType);
                // System.Console.WriteLine($"Potentially create: {potentialSubtypeName} - {createSubType} - {items != null}");

                var property = SyntaxFactory.PropertyDeclaration(
                        typeSyntax,
                        SyntaxFactory.Identifier(CsharpNamingExtensions.SnakeCaseToCamel(propertyName)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                    .AddAttributeLists(attributes);

                properties.Add(property);

                if (createSubType && items != null)
                {
                    subTypes.Add((potentialSubtypeName, items));
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
