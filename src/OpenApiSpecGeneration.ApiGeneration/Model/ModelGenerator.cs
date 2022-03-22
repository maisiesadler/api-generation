using System.Diagnostics.CodeAnalysis;
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
                var (record, subtypes) = TryGenerateRecord(name, GetProperties(openApiComponentSchema.properties));
                yield return record;

                foreach (var (subtypename, subtype) in subtypes)
                {
                    var (subtypeRecord, heello) = TryGenerateRecord(subtypename, GetProperties(subtype.properties));
                    yield return subtypeRecord;

                    foreach (var (_subtypename, _subtype) in heello)
                    {
                        var (_subtypeRecord, _) = TryGenerateRecord(_subtypename, GetProperties(_subtype.properties));
                        yield return _subtypeRecord;
                    }
                }
            }
        }

        private static IEnumerable<(string propertyName, string propertyType, OpenApiComponentPropertyType? items)>
            GetProperties(IReadOnlyDictionary<string, OpenApiComponentProperty>? openApiProperties)
        {
            if (openApiProperties == null) yield break;

            foreach (var (propertyName, openApiProperty) in openApiProperties)
            {
                var type = openApiProperty.type;
                if (type == null)
                    type = openApiProperty.Ref?.Split("/").Last();

                if (type == null)
                    throw new InvalidOperationException($"{propertyName} missing property type");

                yield return (propertyName, type, openApiProperty.items);
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
                var createSubType = ShouldCreateSubType(propertyType, items?.type);
                var typeSyntax = ParseTypeSyntax(propertyType);
                // System.Console.WriteLine($"Potentially create: {potentialSubtypeName} - {createSubType} - {items != null}");

                if (propertyType == "array")
                {
                    var elementType = createSubType
                        ? ParseTypeSyntax(potentialSubtypeName)
                        : ParseTypeSyntax(items?.type ?? throw new InvalidOperationException("Array element does not have type"));

                    typeSyntax = SyntaxFactory.ArrayType(elementType)
                        .WithRankSpecifiers(
                            SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                               SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.OmittedArraySizeExpression()))));
                }

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

        private static bool TryGetPredefinedTypeSyntax(string? propertyType, [NotNullWhen(true)] out PredefinedTypeSyntax? predefinedTypeSyntax)
        {
            switch (propertyType)
            {
                case "integer":
                {
                    predefinedTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
                    return true;
                }
                case "string":
                {
                    predefinedTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
                    return true;
                }
                case "boolean":
                {
                    predefinedTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));
                    return true;
                }
                default:
                {
                    predefinedTypeSyntax = null;
                    return false;
                }
            };
        }

        private static TypeSyntax ParseTypeSyntax(string propertyType)
        {
            if (TryGetPredefinedTypeSyntax(propertyType, out var predefinedTypeSyntax))
                return predefinedTypeSyntax;

            return SyntaxFactory.ParseTypeName(propertyType);
        }

        private static bool ShouldCreateSubType(string? propertyType, string? itemsType)
            => propertyType == "object" || propertyType == "array" && !TryGetPredefinedTypeSyntax(itemsType, out _);

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
