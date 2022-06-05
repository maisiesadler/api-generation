using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Definition;

namespace OpenApiSpecGeneration.Model
{
    internal class ModelGenerator
    {
        internal static IEnumerable<RecordDeclarationSyntax> GenerateModels(OpenApiDocument document)
        {
            var definition = DefinitionGenerator.GenerateDefinition(document);
            return GenerateRecords(definition).ToArray();
        }

        private static IEnumerable<RecordDeclarationSyntax> GenerateRecords(Definition.Definition definition)
        {
            foreach (var schemaDefinition in definition.schemas)
            {
                yield return GenerateRecord(schemaDefinition.name, schemaDefinition.properties);
            }
        }

        private static RecordDeclarationSyntax GenerateRecord(
            string schemaDefinitionName,
            PropertyDefinition[] propertyDefinitions)
        {
            var properties = new List<MemberDeclarationSyntax>();

            foreach (var propertyDefinition in propertyDefinitions)
            {
                var typeSyntax = GetTypeSyntax(propertyDefinition);
                var propertyDeclaration = ModelSyntaxGenerator.CreateProperty(typeSyntax, propertyDefinition.propertyName);

                properties.Add(propertyDeclaration);
            }

            return ModelSyntaxGenerator.CreateRecord(schemaDefinitionName, properties.ToArray());
        }

        private static TypeSyntax GetTypeSyntax(
            PropertyDefinition propertyDefinition)
        {
            if (propertyDefinition.createObjectSubType)
            {
                return SyntaxFactory.ParseTypeName(propertyDefinition.potentialSubtypeName);
            }

            if (propertyDefinition.propertyType == "array")
            {
                var elementType = propertyDefinition.createArraySubType
                    ? ParseTypeSyntax(propertyDefinition.potentialSubtypeName)
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
