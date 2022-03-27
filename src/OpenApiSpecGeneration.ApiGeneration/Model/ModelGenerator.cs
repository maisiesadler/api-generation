using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Model
{
    internal class ModelGenerator
    {
        internal static IEnumerable<RecordDeclarationSyntax> GenerateModels(OpenApiDocument document)
        {
            return GenerateRecords(document).ToArray();
        }

        private static IEnumerable<RecordDeclarationSyntax> GenerateRecords(OpenApiDocument document)
        {
            foreach (var (name, openApiComponentSchema) in GetAllSchemas.Execute(document))
            {
                var (record, subtypes) = TryGenerateRecord(name, GetProperties(openApiComponentSchema.Properties));
                yield return record;

                foreach (var (subtypename, subtype) in subtypes)
                {
                    var (subtypeRecord, heello) = TryGenerateRecord(subtypename, GetProperties(subtype.Properties));
                    yield return subtypeRecord;

                    foreach (var (_subtypename, _subtype) in heello)
                    {
                        var (_subtypeRecord, _) = TryGenerateRecord(_subtypename, GetProperties(_subtype.Properties));
                        yield return _subtypeRecord;
                    }
                }
            }
        }

        private static IEnumerable<(string propertyName, string propertyType, OpenApiSchema property)>
            GetProperties(IDictionary<string, OpenApiSchema>? openApiProperties)
        {
            if (openApiProperties == null) yield break;

            foreach (var (propertyName, openApiProperty) in openApiProperties)
            {
                var type = openApiProperty.Type;
                if (type == null)
                    type = openApiProperty.Reference.Id;

                if (type == null)
                    throw new InvalidOperationException($"{propertyName} missing property type");

                yield return (propertyName, type, openApiProperty);
            }
        }

        private static (RecordDeclarationSyntax, IList<(string, OpenApiSchema)>) TryGenerateRecord(
            string name,
            IEnumerable<(string propertyName, string propertyType, OpenApiSchema property)> openApiProperties)
        {
            var properties = new List<MemberDeclarationSyntax>();
            var subTypes = new List<(string, OpenApiSchema)>();

            foreach (var (propertyName, propertyType, property) in openApiProperties)
            {
                var attributes = SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        JsonPropertyNameAttributeSyntax(propertyName)
                    )
                );

                var potentialSubtypeName = name + CsharpNamingExtensions.SnakeCaseToCamel(propertyName) + "SubType";
                var createArraySubType = ShouldCreateArraySubType(propertyType, property);
                var createObjectSubType = ShouldCreateObjectSubType(propertyType, property);
                var typeSyntax = createObjectSubType
                    ? SyntaxFactory.ParseTypeName(potentialSubtypeName)
                    : TryGetLocalReference(propertyType, property, out var localSyntax)
                        ? localSyntax
                        : ParseTypeSyntax(propertyType);
                // System.Console.WriteLine($"Potentially create: {potentialSubtypeName} - {createSubType} - {items != null}");

                if (propertyType == "array")
                {
                    var elementType = createArraySubType
                        ? ParseTypeSyntax(potentialSubtypeName)
                        : ParseTypeSyntax(property.Items?.Type ?? throw new InvalidOperationException("Array element does not have type"));

                    typeSyntax = SyntaxFactory.ArrayType(elementType)
                        .WithRankSpecifiers(
                            SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                               SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.OmittedArraySizeExpression()))));
                }

                var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                        typeSyntax,
                        SyntaxFactory.Identifier(CsharpNamingExtensions.SnakeCaseToCamel(propertyName)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                    .AddAttributeLists(attributes);

                properties.Add(propertyDeclaration);

                if (createObjectSubType)
                {
                    subTypes.Add((potentialSubtypeName, property));
                }

                if (createArraySubType)
                {
                    subTypes.Add((potentialSubtypeName, property.Items));
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

        private static bool ShouldCreateObjectSubType(string? propertyType, OpenApiSchema property)
            => propertyType == "object" && property.Reference == null && property.Properties?.Any() == true;

        private static bool TryGetLocalReference(string? propertyType, OpenApiSchema property, [NotNullWhen(true)] out TypeSyntax? localReference)
        {
            if (propertyType != "object" || property.Reference == null)
            {
                localReference = null;
                return false;
            }

            localReference = SyntaxFactory.ParseTypeName(property.Reference.Id);
            return true;
        }

        private static bool ShouldCreateArraySubType(string? propertyType, OpenApiSchema property)
            => propertyType == "array" && !TryGetPredefinedTypeSyntax(property.Items?.Type, out _);

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
