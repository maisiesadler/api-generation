// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text.Json;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using Xunit;

// namespace OpenApiSpecGeneration.ApiGeneration.Test;

// public class GenerateModelsTests
// {
//     [Fact]
//     public void SuccessfulTypeModelCreated()
//     {
//         // Arrange
//         var toDoItemProperties = new Dictionary<string, OpenApiComponentProperty>
//         {
//             { "id", new OpenApiComponentProperty("integer", default, default, default) },
//         };
//         var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
//         {
//             { "ToDoItem", new OpenApiComponentSchema("object", toDoItemProperties) }
//         };
//         var components = new OpenApiComponent(componentSchemas);
//         var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

//         // Act
//         var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec);

//         // Assert
//         var recordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);
//         Assert.Equal("ToDoItem", recordDeclarationSyntax.Identifier.Value);
//         var classModifier = Assert.Single(recordDeclarationSyntax.Modifiers);
//         Assert.Equal("public", classModifier.Value);

//         Assert.Equal("{", recordDeclarationSyntax.OpenBraceToken.Value);
//         Assert.Equal("}", recordDeclarationSyntax.CloseBraceToken.Value);
//         var memberDeclarationSyntax = Assert.Single(recordDeclarationSyntax.Members);
//         var propertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(memberDeclarationSyntax);
//         Assert.Equal("Id", propertyDeclarationSyntax.Identifier.Value);
//         var methodModifier = Assert.Single(propertyDeclarationSyntax.Modifiers);
//         Assert.Equal("public", methodModifier.Value);
//         var predefinedTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(propertyDeclarationSyntax.Type);
//         Assert.Equal("int", predefinedTypeSyntax.Keyword.Value);
//     }

//     [Fact]
//     public void ModelHasJsonPropertyNameAttribute()
//     {
//         // Arrange
//         var toDoItemProperties = new Dictionary<string, OpenApiComponentProperty>
//         {
//             { "id", new OpenApiComponentProperty("integer",default,  default, default) },
//         };
//         var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
//         {
//             { "ToDoItem", new OpenApiComponentSchema("object", toDoItemProperties) }
//         };
//         var components = new OpenApiComponent(componentSchemas);
//         var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

//         // Act
//         var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec);

//         // Assert
//         var recordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);
//         var memberDeclarationSyntax = Assert.Single(recordDeclarationSyntax.Members);
//         var attributeListSyntax = Assert.Single(memberDeclarationSyntax.AttributeLists);
//         var attributeSyntax = Assert.Single(attributeListSyntax.Attributes);
//         var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(attributeSyntax.Name);
//         Assert.Equal("JsonPropertyName", identifierNameSyntax.Identifier.Value);
//         Assert.NotNull(attributeSyntax.ArgumentList);
//         Assert.Equal("(", attributeSyntax.ArgumentList!.OpenParenToken.Value);
//         Assert.Equal(")", attributeSyntax.ArgumentList!.CloseParenToken.Value);
//         var argument = Assert.Single(attributeSyntax.ArgumentList!.Arguments);
//         var literalExpressionSyntax = Assert.IsType<LiteralExpressionSyntax>(argument.Expression);
//         Assert.Equal("\"id\"", literalExpressionSyntax.Token.Value);
//     }

//     [Theory]
//     [InlineData("integer", "int")]
//     [InlineData("string", "string")]
//     [InlineData("boolean", "bool")]
//     public void SupportedPropertyTypesConvertedCorrectly(string openApiType, string expectedCsharpType)
//     {
//         // Arrange
//         var toDoItemProperties = new Dictionary<string, OpenApiComponentProperty>
//         {
//             { "id", new OpenApiComponentProperty(openApiType,default,  default, default) },
//         };
//         var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
//         {
//             { "ToDoItem", new OpenApiComponentSchema("object", toDoItemProperties) }
//         };
//         var components = new OpenApiComponent(componentSchemas);
//         var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

//         // Act
//         var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec);

//         // Assert
//         var recordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);
//         var memberDeclarationSyntax = Assert.Single(recordDeclarationSyntax.Members);
//         var propertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(memberDeclarationSyntax);
//         var predefinedTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(propertyDeclarationSyntax.Type);
//         Assert.Equal(expectedCsharpType, predefinedTypeSyntax.Keyword.Value);
//     }

//     [Fact]
//     public void PropertyWithNestedTypeGeneratesSubtype()
//     {
//         // Arrange
//         var properties = new Dictionary<string, OpenApiComponentProperty>
//         {
//             { "name", new OpenApiComponentProperty("integer", default, default, default) },
//         };
//         var toDoItemProperties = new Dictionary<string, OpenApiComponentProperty>
//         {
//             { "id", new OpenApiComponentProperty("array", new OpenApiComponentPropertyType(properties, null), default, default) },
//         };
//         var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
//         {
//             { "ToDoItem", new OpenApiComponentSchema("object", toDoItemProperties) }
//         };
//         var components = new OpenApiComponent(componentSchemas);
//         var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

//         // Act
//         var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec).ToArray();

//         // Assert
//         Assert.Equal(2, recordDeclarationSyntaxes.Length);

//         var typeSyntax = recordDeclarationSyntaxes[0];
//         Assert.Equal("ToDoItem", typeSyntax.Identifier.Value);
//         var typeClassModifier = Assert.Single(typeSyntax.Modifiers);
//         Assert.Equal("public", typeClassModifier.Value);

//         Assert.Equal("{", typeSyntax.OpenBraceToken.Value);
//         Assert.Equal("}", typeSyntax.CloseBraceToken.Value);
//         var typeMemberDeclarationSyntax = Assert.Single(typeSyntax.Members);
//         var typePropertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(typeMemberDeclarationSyntax);
//         Assert.Equal("Id", typePropertyDeclarationSyntax.Identifier.Value);
//         var typePropertyMethodModifier = Assert.Single(typePropertyDeclarationSyntax.Modifiers);
//         Assert.Equal("public", typePropertyMethodModifier.Value);
//         var typePropertyArrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(typePropertyDeclarationSyntax.Type);
//         var typePropertyArrayElementType = Assert.IsType<IdentifierNameSyntax>(typePropertyArrayTypeSyntax.ElementType);
//         Assert.Equal("ToDoItemIdSubType", typePropertyArrayElementType.Identifier.Value);

//         var subTypeSyntax = recordDeclarationSyntaxes[1];
//         Assert.Equal("ToDoItemIdSubType", subTypeSyntax.Identifier.Value);
//         var subTypeClassModifier = Assert.Single(subTypeSyntax.Modifiers);
//         Assert.Equal("public", subTypeClassModifier.Value);

//         Assert.Equal("{", subTypeSyntax.OpenBraceToken.Value);
//         Assert.Equal("}", subTypeSyntax.CloseBraceToken.Value);
//         var subtypeMemberDeclarationSyntax = Assert.Single(subTypeSyntax.Members);
//         var subTypePropertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(subtypeMemberDeclarationSyntax);
//         Assert.Equal("Name", subTypePropertyDeclarationSyntax.Identifier.Value);
//         var subTypePropertyMethodModifier = Assert.Single(subTypePropertyDeclarationSyntax.Modifiers);
//         Assert.Equal("public", subTypePropertyMethodModifier.Value);
//         var subTypePropertyTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(subTypePropertyDeclarationSyntax.Type);
//         Assert.Equal("int", subTypePropertyTypeSyntax.Keyword.Value);
//     }

//     [Fact]
//     public void SupportArrays()
//     {
//         // Arrange
//         var toDoItemProperties = new Dictionary<string, OpenApiComponentProperty>
//         {
//             { "things", new OpenApiComponentProperty("array", new OpenApiComponentPropertyType(null, "string"),  default, default) },
//         };
//         var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
//         {
//             { "ToDoItem", new OpenApiComponentSchema("object", toDoItemProperties) }
//         };
//         var components = new OpenApiComponent(componentSchemas);
//         var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

//         // Act
//         var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec).ToList();

//         // Assert
//         var typeRecordDeclarationSyntax = Assert.Single(recordDeclarationSyntaxes);

//         Assert.Equal("ToDoItem", typeRecordDeclarationSyntax.Identifier.Value);
//         var memberDeclarationSyntax = Assert.Single(typeRecordDeclarationSyntax.Members);
//         var propertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(memberDeclarationSyntax);
//         Assert.Equal("Things", propertyDeclarationSyntax.Identifier.Value);

//         var arrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(propertyDeclarationSyntax.Type);
//         var predefinedTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(arrayTypeSyntax.ElementType);
//         Assert.Equal("string", predefinedTypeSyntax.Keyword.Value);
//     }

//     // public void PathSchema()
//     // {
//     //     // Arrange
//     //     var openApiContentSchema = new OpenApiContentSchema("array", new Dictionary<string, string>
//     //     {
//     //         { "$ref", "#/components/schemas/ToDoItem" },
//     //     });
//     //     var openApiResponse = new OpenApiResponse("Success", new Dictionary<string, OpenApiContent>
//     //     {
//     //         { "text/plain", new OpenApiContent(openApiContentSchema) }
//     //     });
//     //     var openApiResponses = new Dictionary<string, OpenApiResponse> { { "200", openApiResponse } };
//     //     var apiTestPath = new OpenApiPath
//     //     {
//     //         { "get", new OpenApiMethod { responses = openApiResponses } },
//     //     };
//     //     var paths = new Dictionary<string, OpenApiPath>
//     //     {
//     //         { "/api/test", apiTestPath },
//     //     };

//     //     var componentSchemas = new Dictionary<string, OpenApiComponentSchema>();
//     //     var components = new OpenApiComponent(componentSchemas);
//     //     var spec = new OpenApiSpec(paths, components);

//     //     // Act
//     //     var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

//     //     // Assert
//     //     var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
//     //     Assert.Equal("ApiTest", classDeclarationSyntax.Identifier.Value);
//     //     var classModifier = Assert.Single(classDeclarationSyntax.Modifiers);
//     //     Assert.Equal("public", classModifier.Value);

//     //     Assert.Equal(2, classDeclarationSyntax.Members.Count);
//     //     var memberDeclarationSyntax = classDeclarationSyntax.Members[1]; // first is ctor, skip here
//     //     var methodDeclarationSyntax = Assert.IsType<MethodDeclarationSyntax>(memberDeclarationSyntax);
//     //     Assert.Equal("Get", methodDeclarationSyntax.Identifier.Value);
//     //     var methodModifier = Assert.Single(methodDeclarationSyntax.Modifiers);
//     //     Assert.Equal("public", methodModifier.Value);
//     // }
// }
