using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateModelsTests
{
    [Fact]
    public void SuccessfulTypeModelCreated()
    {
        // Arrange
        var toDoItemProperties = new Dictionary<string, OpenApiComponentProperty>
        {
            { "id", new OpenApiComponentProperty("integer", default, default) },
        };
        var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
        {
            { "ToDoItem", new OpenApiComponentSchema("object", toDoItemProperties) }
        };
        var components = new OpenApiComponent(componentSchemas);
        var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec);

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
        var predefinedTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(propertyDeclarationSyntax.Type);
        Assert.Equal("int", predefinedTypeSyntax.Keyword.Value);
    }

    [Fact]
    public void ModelHasJsonPropertyNameAttribute()
    {
        // Arrange
        var toDoItemProperties = new Dictionary<string, OpenApiComponentProperty>
        {
            { "id", new OpenApiComponentProperty("integer", default, default) },
        };
        var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
        {
            { "ToDoItem", new OpenApiComponentSchema("object", toDoItemProperties) }
        };
        var components = new OpenApiComponent(componentSchemas);
        var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec);

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

    // public void PathSchema()
    // {
    //     // Arrange
    //     var openApiContentSchema = new OpenApiContentSchema("array", new Dictionary<string, string>
    //     {
    //         { "$ref", "#/components/schemas/ToDoItem" },
    //     });
    //     var openApiResponse = new OpenApiResponse("Success", new Dictionary<string, OpenApiContent>
    //     {
    //         { "text/plain", new OpenApiContent(openApiContentSchema) }
    //     });
    //     var openApiResponses = new Dictionary<string, OpenApiResponse> { { "200", openApiResponse } };
    //     var apiTestPath = new OpenApiPath
    //     {
    //         { "get", new OpenApiMethod { responses = openApiResponses } },
    //     };
    //     var paths = new Dictionary<string, OpenApiPath>
    //     {
    //         { "/api/test", apiTestPath },
    //     };

    //     var componentSchemas = new Dictionary<string, OpenApiComponentSchema>();
    //     var components = new OpenApiComponent(componentSchemas);
    //     var spec = new OpenApiSpec(paths, components);

    //     // Act
    //     var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

    //     // Assert
    //     var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
    //     Assert.Equal("ApiTest", classDeclarationSyntax.Identifier.Value);
    //     var classModifier = Assert.Single(classDeclarationSyntax.Modifiers);
    //     Assert.Equal("public", classModifier.Value);

    //     Assert.Equal(2, classDeclarationSyntax.Members.Count);
    //     var memberDeclarationSyntax = classDeclarationSyntax.Members[1]; // first is ctor, skip here
    //     var methodDeclarationSyntax = Assert.IsType<MethodDeclarationSyntax>(memberDeclarationSyntax);
    //     Assert.Equal("Get", methodDeclarationSyntax.Identifier.Value);
    //     var methodModifier = Assert.Single(methodDeclarationSyntax.Modifiers);
    //     Assert.Equal("public", methodModifier.Value);
    // }
}
