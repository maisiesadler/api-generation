using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Generatable;

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
            foreach (var schemaDefinition in SchemaDefinitionGenerator.Execute(document))
            {
                foreach (var record in GenerateNestedRecords(schemaDefinition))
                    yield return record;
            }
        }

        private static IEnumerable<RecordDeclarationSyntax> GenerateNestedRecords(SchemaDefinition schemaDefinition)
        {
            var (record, subtypes) = TryGenerateRecord(schemaDefinition);
            yield return record;

            foreach (var subTypeSchemaDefinition in subtypes)
            {
                foreach (var subTypeRecord in GenerateNestedRecords(subTypeSchemaDefinition))
                    yield return subTypeRecord;
            }
        }

        private static (RecordDeclarationSyntax, IList<SchemaDefinition>) TryGenerateRecord(
            SchemaDefinition schemaDefinition)
        {
            var properties = new List<MemberDeclarationSyntax>();
            var subTypes = new List<SchemaDefinition>();

            foreach (var propertyDefinition in schemaDefinition.GetProperties())
            {
                var (propertyName, propertyType, property) = propertyDefinition;

                var attributes = SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        JsonPropertyNameAttributeSyntax(propertyName)
                    )
                );

                var potentialSubtypeName = schemaDefinition.name + CsharpNamingExtensions.SnakeCaseToCamel(propertyName) + "SubType";
                var createArraySubType = ShouldCreateArraySubType(propertyType, property);
                var createObjectSubType = ShouldCreateObjectSubType(propertyType, property);
                var typeSyntax = GetTypeSyntax(createObjectSubType, createArraySubType, potentialSubtypeName, propertyDefinition);

                var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                        SyntaxFactory.NullableType(typeSyntax),
                        SyntaxFactory.Identifier(CsharpNamingExtensions.SnakeCaseToCamel(propertyName)))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)))
                    .AddAttributeLists(attributes);

                properties.Add(propertyDeclaration);

                if (createObjectSubType)
                {
                    subTypes.Add(new SchemaDefinition(potentialSubtypeName, property));
                }

                if (createArraySubType)
                {
                    subTypes.Add(new SchemaDefinition(potentialSubtypeName, property.Items));
                }
            }

            var record = SyntaxFactory.RecordDeclaration(
                 SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                  SyntaxFactory.Identifier(schemaDefinition.name)
            )
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
            .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(
                properties.ToArray()
            ));

            return (record, subTypes);
        }

        private static TypeSyntax GetTypeSyntax(
            bool createObjectSubType,
            bool createArraySubType,
            string potentialSubtypeName,
            PropertyDefinition propertyDefinition)
        {
            var (propertyName, propertyType, property) = propertyDefinition;

            if (createObjectSubType)
            {
                return SyntaxFactory.ParseTypeName(potentialSubtypeName);
            }

            if (propertyType == "array")
            {
                var elementType = createArraySubType
                    ? ParseTypeSyntax(potentialSubtypeName)
                    : ParseTypeSyntax(property.Items?.Type ?? throw new InvalidOperationException("Array element does not have type"));

                return SyntaxFactory.ArrayType(elementType)
                    .WithRankSpecifiers(
                        SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                           SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression()))));
            }

            return TryGetLocalReference(propertyType, property, out var localSyntax)
                ? localSyntax
                : ParseTypeSyntax(propertyType);
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
