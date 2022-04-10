using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateModelsTests
{
    [Fact]
    public void SuccessfulTypeModelCreated()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                { "id", new OpenApiSchema { Type ="integer" } },
            },
        };

        var document = OpenApiMockBuilder.BuildDocument()
            .WithComponentSchema("ToDoItem", responseSchema);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document);

        // Assert
        var recordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);
        Assert.Equal("ToDoItem", recordDeclarationSyntax.Identifier.Value);
        var classModifier = Assert.Single(recordDeclarationSyntax.Modifiers);
        Assert.Equal("public", classModifier.Value);

        Assert.Equal("{", recordDeclarationSyntax.OpenBraceToken.Value);
        Assert.Equal("}", recordDeclarationSyntax.CloseBraceToken.Value);
        var memberDeclarationSyntax = Assert.Single(recordDeclarationSyntax.Members);
        var propertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(memberDeclarationSyntax);
        Assert.Equal("Id", propertyDeclarationSyntax.Identifier.Value);
        var methodModifier = Assert.Single(propertyDeclarationSyntax.Modifiers);
        Assert.Equal("public", methodModifier.Value);
        var nullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(propertyDeclarationSyntax.Type);
        var predefinedTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(nullableTypeSyntax.ElementType);
        Assert.Equal("int", predefinedTypeSyntax.Keyword.Value);
    }

    [Fact]
    public void ModelHasJsonPropertyNameAttribute()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                { "id", new OpenApiSchema { Type = "integer" } },
            },
        };

        var document = OpenApiMockBuilder.BuildDocument()
            .WithComponentSchema("ToDoItem", responseSchema);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document);

        // Assert
        var recordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);
        var memberDeclarationSyntax = Assert.Single(recordDeclarationSyntax.Members);
        var attributeListSyntax = Assert.Single(memberDeclarationSyntax.AttributeLists);
        var attributeSyntax = Assert.Single(attributeListSyntax.Attributes);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(attributeSyntax.Name);
        Assert.Equal("JsonPropertyName", identifierNameSyntax.Identifier.Value);
        Assert.NotNull(attributeSyntax.ArgumentList);
        Assert.Equal("(", attributeSyntax.ArgumentList!.OpenParenToken.Value);
        Assert.Equal(")", attributeSyntax.ArgumentList!.CloseParenToken.Value);
        var argument = Assert.Single(attributeSyntax.ArgumentList!.Arguments);
        var literalExpressionSyntax = Assert.IsType<LiteralExpressionSyntax>(argument.Expression);
        Assert.Equal("\"id\"", literalExpressionSyntax.Token.Value);
    }

    [Theory]
    [InlineData("integer", "int")]
    [InlineData("string", "string")]
    [InlineData("boolean", "bool")]
    public void SupportedPropertyTypesConvertedCorrectly(string openApiType, string expectedCsharpType)
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                { "id", new OpenApiSchema { Type = openApiType} },
            },
        };

        var document = OpenApiMockBuilder.BuildDocument()
            .WithComponentSchema("ToDoItem", responseSchema);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document);

        // Assert
        var recordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);
        var memberDeclarationSyntax = Assert.Single(recordDeclarationSyntax.Members);
        var propertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(memberDeclarationSyntax);
        var nullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(propertyDeclarationSyntax.Type);
        var predefinedTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(nullableTypeSyntax.ElementType);
        Assert.Equal(expectedCsharpType, predefinedTypeSyntax.Keyword.Value);
    }

    [Fact]
    public void PropertyWithNestedTypeGeneratesSubtype()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "id",
                    new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                { "name", new OpenApiSchema { Type = "integer" } },
                            },
                        },
                    }
                },
            },
        };

        var document = OpenApiMockBuilder.BuildDocument()
            .WithComponentSchema("ToDoItem", responseSchema);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document).ToArray();

        // Assert
        Assert.Equal(2, recordDeclarationSyntaxes.Length);

        var typeSyntax = recordDeclarationSyntaxes[0];
        Assert.Equal("ToDoItem", typeSyntax.Identifier.Value);
        var typeClassModifier = Assert.Single(typeSyntax.Modifiers);
        Assert.Equal("public", typeClassModifier.Value);

        Assert.Equal("{", typeSyntax.OpenBraceToken.Value);
        Assert.Equal("}", typeSyntax.CloseBraceToken.Value);
        var typeMemberDeclarationSyntax = Assert.Single(typeSyntax.Members);
        var typePropertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(typeMemberDeclarationSyntax);
        Assert.Equal("Id", typePropertyDeclarationSyntax.Identifier.Value);
        var typePropertyMethodModifier = Assert.Single(typePropertyDeclarationSyntax.Modifiers);
        Assert.Equal("public", typePropertyMethodModifier.Value);
        var nullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(typePropertyDeclarationSyntax.Type);
        var typePropertyArrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(nullableTypeSyntax.ElementType);
        var typePropertyArrayElementType = Assert.IsType<IdentifierNameSyntax>(typePropertyArrayTypeSyntax.ElementType);
        Assert.Equal("ToDoItemIdSubType", typePropertyArrayElementType.Identifier.Value);

        var subTypeSyntax = recordDeclarationSyntaxes[1];
        Assert.Equal("ToDoItemIdSubType", subTypeSyntax.Identifier.Value);
        var subTypeClassModifier = Assert.Single(subTypeSyntax.Modifiers);
        Assert.Equal("public", subTypeClassModifier.Value);

        Assert.Equal("{", subTypeSyntax.OpenBraceToken.Value);
        Assert.Equal("}", subTypeSyntax.CloseBraceToken.Value);
        var subtypeMemberDeclarationSyntax = Assert.Single(subTypeSyntax.Members);
        var subTypePropertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(subtypeMemberDeclarationSyntax);
        Assert.Equal("Name", subTypePropertyDeclarationSyntax.Identifier.Value);
        var subTypePropertyMethodModifier = Assert.Single(subTypePropertyDeclarationSyntax.Modifiers);
        Assert.Equal("public", subTypePropertyMethodModifier.Value);
        var subTypeNullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(subTypePropertyDeclarationSyntax.Type);
        var subTypePropertyTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(subTypeNullableTypeSyntax.ElementType);
        Assert.Equal("int", subTypePropertyTypeSyntax.Keyword.Value);
    }

    [Fact]
    public void SupportArrays()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                { "things", new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "string" },
                    }
                },
            },
        };

        var document = OpenApiMockBuilder.BuildDocument()
            .WithComponentSchema("ToDoItem", responseSchema);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document).ToList();

        // Assert
        var typeRecordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);

        Assert.Equal("ToDoItem", typeRecordDeclarationSyntax.Identifier.Value);
        var memberDeclarationSyntax = Assert.Single(typeRecordDeclarationSyntax.Members);
        var propertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(memberDeclarationSyntax);
        Assert.Equal("Things", propertyDeclarationSyntax.Identifier.Value);

        var nullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(propertyDeclarationSyntax.Type);
        var arrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(nullableTypeSyntax.ElementType);
        var predefinedTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(arrayTypeSyntax.ElementType);
        Assert.Equal("string", predefinedTypeSyntax.Keyword.Value);
    }

    [Fact]
    public void PathRequestSchemaGenerated()
    {
        // Arrange
        var requestBodySchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                { "id", new OpenApiSchema { Type = "string" }},
            }
        };

        var requestBody = OpenApiMockBuilder.BuildRequestBody("This is the request")
             .AddContent("text/plain", requestBodySchema);

        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(
                OperationType.Post,
                operation => operation.RequestBody = requestBody
            );

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document);

        // Assert
        var recordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);
        Assert.Equal("ApiTestPostTextPlainRequest", recordDeclarationSyntax.Identifier.Value);
        var classModifier = Assert.Single(recordDeclarationSyntax.Modifiers);
        Assert.Equal("public", classModifier.Value);

        Assert.Equal("{", recordDeclarationSyntax.OpenBraceToken.Value);
        Assert.Equal("}", recordDeclarationSyntax.CloseBraceToken.Value);
        var subtypeMemberDeclarationSyntax = Assert.Single(recordDeclarationSyntax.Members);
        var subTypePropertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(subtypeMemberDeclarationSyntax);
        Assert.Equal("Id", subTypePropertyDeclarationSyntax.Identifier.Value);
        var subTypePropertyMethodModifier = Assert.Single(subTypePropertyDeclarationSyntax.Modifiers);
        Assert.Equal("public", subTypePropertyMethodModifier.Value);
        var nullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(subTypePropertyDeclarationSyntax.Type);
        var subTypePropertyTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(nullableTypeSyntax.ElementType);
        Assert.Equal("string", subTypePropertyTypeSyntax.Keyword.Value);
    }

    [Fact]
    public void PathResponseSchemaGenerated()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                { "id", new OpenApiSchema { Type = "string" }},
            }
        };

        var response = OpenApiMockBuilder.BuildResponse("Success")
             .AddContent("text/plain", responseSchema);

        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(
                OperationType.Get,
                operation => operation.Responses.Add("200", response)
            );

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document);

        // Assert
        var recordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);
        Assert.Equal("ApiTestGet200TextPlainResponse", recordDeclarationSyntax.Identifier.Value);
        var classModifier = Assert.Single(recordDeclarationSyntax.Modifiers);
        Assert.Equal("public", classModifier.Value);

        Assert.Equal("{", recordDeclarationSyntax.OpenBraceToken.Value);
        Assert.Equal("}", recordDeclarationSyntax.CloseBraceToken.Value);
        var subtypeMemberDeclarationSyntax = Assert.Single(recordDeclarationSyntax.Members);
        var subTypePropertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(subtypeMemberDeclarationSyntax);
        Assert.Equal("Id", subTypePropertyDeclarationSyntax.Identifier.Value);
        var subTypePropertyMethodModifier = Assert.Single(subTypePropertyDeclarationSyntax.Modifiers);
        Assert.Equal("public", subTypePropertyMethodModifier.Value);
        var subTypeNullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(subTypePropertyDeclarationSyntax.Type);
        var subTypePropertyTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(subTypeNullableTypeSyntax.ElementType);
        Assert.Equal("string", subTypePropertyTypeSyntax.Keyword.Value);
    }

    [Fact]
    public void NestedLocalModel()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "address",
                    new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            {
                                "house_number",
                                new OpenApiSchema
                                {
                                    Type = "integer",
                                }
                            },
                        },
                    }
                },
            },
        };

        var document = OpenApiMockBuilder.BuildDocument()
            .WithComponentSchema("ToDoItem", responseSchema);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document).ToArray();

        // Assert
        Assert.Equal(2, recordDeclarationSyntaxes.Length);

        var recordSyntax0 = recordDeclarationSyntaxes[0];
        Assert.Equal("ToDoItem", recordSyntax0.Identifier.Value);

        var typeMemberDeclarationSyntax0 = Assert.Single(recordSyntax0.Members);
        var typePropertyDeclarationSyntax0 = Assert.IsType<PropertyDeclarationSyntax>(typeMemberDeclarationSyntax0);
        Assert.Equal("Address", typePropertyDeclarationSyntax0.Identifier.Value);
        var typePropertyMethodModifier0 = Assert.Single(typePropertyDeclarationSyntax0.Modifiers);
        Assert.Equal("public", typePropertyMethodModifier0.Value);
        var nullableTypeSyntax0 = Assert.IsType<NullableTypeSyntax>(typePropertyDeclarationSyntax0.Type);
        var typePropertyType0 = Assert.IsType<IdentifierNameSyntax>(nullableTypeSyntax0.ElementType);
        Assert.Equal("ToDoItemAddressSubType", typePropertyType0.Identifier.Value);

        var recordSyntax1 = recordDeclarationSyntaxes[1];
        Assert.Equal("ToDoItemAddressSubType", recordSyntax1.Identifier.Value);

        var typeMemberDeclarationSyntax1 = Assert.Single(recordSyntax1.Members);
        var typePropertyDeclarationSyntax1 = Assert.IsType<PropertyDeclarationSyntax>(typeMemberDeclarationSyntax1);
        Assert.Equal("HouseNumber", typePropertyDeclarationSyntax1.Identifier.Value);
        var typePropertyMethodModifier1 = Assert.Single(typePropertyDeclarationSyntax1.Modifiers);
        Assert.Equal("public", typePropertyMethodModifier1.Value);
        var nullableTypeSyntax1 = Assert.IsType<NullableTypeSyntax>(typePropertyDeclarationSyntax1.Type);
        var typePropertyType1 = Assert.IsType<PredefinedTypeSyntax>(nullableTypeSyntax1.ElementType);
        Assert.Equal("int", typePropertyType1.Keyword.Value);
    }

    [Fact]
    public void NestedModelReferencingAnotherSchema()
    {
        // Arrange
        var propertySchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "house_number",
                    new OpenApiSchema
                    {
                        Type = "integer",
                    }
                },
            },
        };
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "address",
                    new OpenApiSchema
                    {
                        Type = "object",
                        Reference = new OpenApiReference
                        {
                            Id = "Address",
                        }
                    }
                },
            },
        };

        var document = OpenApiMockBuilder.BuildDocument()
            .WithComponentSchema("Address", propertySchema)
            .WithComponentSchema("ToDoItem", responseSchema);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document).ToArray();

        // Assert
        Assert.Equal(2, recordDeclarationSyntaxes.Length);

        var recordSyntax0 = recordDeclarationSyntaxes[0];
        Assert.Equal("Address", recordSyntax0.Identifier.Value);

        var typeMemberDeclarationSyntax0 = Assert.Single(recordSyntax0.Members);
        var typePropertyDeclarationSyntax0 = Assert.IsType<PropertyDeclarationSyntax>(typeMemberDeclarationSyntax0);
        Assert.Equal("HouseNumber", typePropertyDeclarationSyntax0.Identifier.Value);
        var typePropertyMethodModifier0 = Assert.Single(typePropertyDeclarationSyntax0.Modifiers);
        Assert.Equal("public", typePropertyMethodModifier0.Value);
        var nullableTypeSyntax0 = Assert.IsType<NullableTypeSyntax>(typePropertyDeclarationSyntax0.Type);
        var typePropertyType0 = Assert.IsType<PredefinedTypeSyntax>(nullableTypeSyntax0.ElementType);
        Assert.Equal("int", typePropertyType0.Keyword.Value);


        var recordSyntax1 = recordDeclarationSyntaxes[1];
        Assert.Equal("ToDoItem", recordSyntax1.Identifier.Value);

        var typeMemberDeclarationSyntax1 = Assert.Single(recordSyntax1.Members);
        var typePropertyDeclarationSyntax1 = Assert.IsType<PropertyDeclarationSyntax>(typeMemberDeclarationSyntax1);
        Assert.Equal("Address", typePropertyDeclarationSyntax1.Identifier.Value);
        var typePropertyMethodModifier1 = Assert.Single(typePropertyDeclarationSyntax1.Modifiers);
        Assert.Equal("public", typePropertyMethodModifier1.Value);
        var nullableTypeSyntax1 = Assert.IsType<NullableTypeSyntax>(typePropertyDeclarationSyntax1.Type);
        var typePropertyType1 = Assert.IsType<IdentifierNameSyntax>(nullableTypeSyntax1.ElementType);
        Assert.Equal("Address", typePropertyType1.Identifier.Value);
    }
}
