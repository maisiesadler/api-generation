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
            var (record, subtypes) = GenerateRecord(schemaDefinition);
            yield return record;

            foreach (var subTypeSchemaDefinition in subtypes)
            {
                foreach (var subTypeRecord in GenerateNestedRecords(subTypeSchemaDefinition))
                    yield return subTypeRecord;
            }
        }

        private static (RecordDeclarationSyntax, IList<SchemaDefinition>) GenerateRecord(
            SchemaDefinition schemaDefinition)
        {
            var properties = new List<MemberDeclarationSyntax>();
            var subTypes = new List<SchemaDefinition>();

            foreach (var propertyDefinition in PropertyDefinitionGenerator.Execute(schemaDefinition.schema.Properties))
            {
                var potentialSubtypeName = schemaDefinition.name + CsharpNamingExtensions.SnakeCaseToCamel(propertyDefinition.propertyName) + "SubType";
                var typeSyntax = GetTypeSyntax(potentialSubtypeName, propertyDefinition);

                var propertyDeclaration = ModelSyntaxGenerator.CreateProperty(typeSyntax, propertyDefinition.propertyName);

                properties.Add(propertyDeclaration);

                if (propertyDefinition.createObjectSubType)
                {
                    subTypes.Add(new SchemaDefinition(potentialSubtypeName, propertyDefinition.property));
                }

                if (propertyDefinition.createArraySubType)
                {
                    subTypes.Add(new SchemaDefinition(potentialSubtypeName, propertyDefinition.property.Items));
                }
            }

            var record = ModelSyntaxGenerator.CreateRecord(schemaDefinition.name, properties.ToArray());

            return (record, subTypes);
        }

        private static TypeSyntax GetTypeSyntax(
            string potentialSubtypeName,
            PropertyDefinition propertyDefinition)
        {
            if (propertyDefinition.createObjectSubType)
            {
                return SyntaxFactory.ParseTypeName(potentialSubtypeName);
            }

            if (propertyDefinition.propertyType == "array")
            {
                var elementType = propertyDefinition.createArraySubType
                    ? ParseTypeSyntax(potentialSubtypeName)
                    : ParseTypeSyntax(propertyDefinition.property.Items?.Type ?? throw new InvalidOperationException("Array element does not have type"));

                return SyntaxFactory.ArrayType(elementType)
                    .WithRankSpecifiers(
                        SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                           SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression()))));
            }

            return TryGetLocalReference(propertyDefinition.propertyType, propertyDefinition.property, out var localSyntax)
                ? localSyntax
                : ParseTypeSyntax(propertyDefinition.propertyType);
        }

        private static TypeSyntax ParseTypeSyntax(string propertyType)
        {
            if (CsharpTypeExtensions.TryGetPredefinedTypeSyntax(propertyType, out var predefinedTypeSyntax))
                return predefinedTypeSyntax;

            return SyntaxFactory.ParseTypeName(propertyType);
        }

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
    }
}
