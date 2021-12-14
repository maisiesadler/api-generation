using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.Entities;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateModelsTestDataNestedComponentSchemaTests
{
    [Fact]
    public void GeneratedModelNamesCorrect()
    {
        // Arrange
        var componentSchema = TestDataCache.Get<OpenApiComponentSchema>("NestedComponentSchema");
        var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
        {
            { "ToDoItem", componentSchema },
        };
        var components = new OpenApiComponent(componentSchemas);

        var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec).ToArray();

        // Assert
        Assert.Equal(3, recordDeclarationSyntaxes.Length);

        var recordSyntax0 = recordDeclarationSyntaxes[0];
        Assert.Equal("ToDoItem", recordSyntax0.Identifier.Value);
        var recordSyntax1 = recordDeclarationSyntaxes[1];
        Assert.Equal("ToDoItemReportSubType", recordSyntax1.Identifier.Value);
        var recordSyntax2 = recordDeclarationSyntaxes[2];
        Assert.Equal("ToDoItemReportSubTypeAccountHoldersSubType", recordSyntax2.Identifier.Value);
    }

    [Fact]
    public void RootModelPropertiesCorrect()
    {
        // Arrange
        var componentSchema = TestDataCache.Get<OpenApiComponentSchema>("NestedComponentSchema");
        var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
        {
            { "ToDoItem", componentSchema },
        };
        var components = new OpenApiComponent(componentSchemas);

        var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec).ToArray();

        // Assert
        Assert.Equal(3, recordDeclarationSyntaxes.Length);

        var recordSyntax0 = recordDeclarationSyntaxes[0];
        Assert.Equal("ToDoItem", recordSyntax0.Identifier.Value);

        Assert.Equal("{", recordSyntax0.OpenBraceToken.Value);
        Assert.Equal("}", recordSyntax0.CloseBraceToken.Value);
        var typeMemberDeclarationSyntax = Assert.Single(recordSyntax0.Members);
        var typePropertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(typeMemberDeclarationSyntax);
        Assert.Equal("Report", typePropertyDeclarationSyntax.Identifier.Value);
        var typePropertyMethodModifier = Assert.Single(typePropertyDeclarationSyntax.Modifiers);
        Assert.Equal("public", typePropertyMethodModifier.Value);
        var typePropertyArrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(typePropertyDeclarationSyntax.Type);
        var typePropertyArrayElementType = Assert.IsType<IdentifierNameSyntax>(typePropertyArrayTypeSyntax.ElementType);
        Assert.Equal("ToDoItemReportSubType", typePropertyArrayElementType.Identifier.Value);
    }

    [Fact]
    public void NestedModelPropertiesCorrect()
    {
        // Arrange
        var componentSchema = TestDataCache.Get<OpenApiComponentSchema>("NestedComponentSchema");
        var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
        {
            { "ToDoItem", componentSchema },
        };
        var components = new OpenApiComponent(componentSchemas);

        var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec).ToArray();

        // Assert
        Assert.Equal(3, recordDeclarationSyntaxes.Length);

        var recordSyntax1 = recordDeclarationSyntaxes[1];
        Assert.Equal("ToDoItemReportSubType", recordSyntax1.Identifier.Value);

        Assert.Equal("{", recordSyntax1.OpenBraceToken.Value);
        Assert.Equal("}", recordSyntax1.CloseBraceToken.Value);
        var typeMemberDeclarationSyntax = Assert.Single(recordSyntax1.Members);
        var typePropertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(typeMemberDeclarationSyntax);
        Assert.Equal("AccountHolders", typePropertyDeclarationSyntax.Identifier.Value);
        var typePropertyMethodModifier = Assert.Single(typePropertyDeclarationSyntax.Modifiers);
        Assert.Equal("public", typePropertyMethodModifier.Value);
        var typePropertyArrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(typePropertyDeclarationSyntax.Type);
        var typePropertyArrayElementType = Assert.IsType<IdentifierNameSyntax>(typePropertyArrayTypeSyntax.ElementType);
        Assert.Equal("ToDoItemReportSubTypeAccountHoldersSubType", typePropertyArrayElementType.Identifier.Value);
    }

    [Fact]
    public void NestedNestedModelPropertiesCorrect()
    {
        // Arrange
        var componentSchema = TestDataCache.Get<OpenApiComponentSchema>("NestedComponentSchema");
        var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
        {
            { "ToDoItem", componentSchema },
        };
        var components = new OpenApiComponent(componentSchemas);

        var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(spec).ToArray();

        // Assert
        Assert.Equal(3, recordDeclarationSyntaxes.Length);

        var recordSyntax2 = recordDeclarationSyntaxes[2];
        Assert.Equal("ToDoItemReportSubTypeAccountHoldersSubType", recordSyntax2.Identifier.Value);

        Assert.Equal("{", recordSyntax2.OpenBraceToken.Value);
        Assert.Equal("}", recordSyntax2.CloseBraceToken.Value);
        var typeMemberDeclarationSyntax = Assert.Single(recordSyntax2.Members);
        var typePropertyDeclarationSyntax = Assert.IsType<PropertyDeclarationSyntax>(typeMemberDeclarationSyntax);
        Assert.Equal("Name", typePropertyDeclarationSyntax.Identifier.Value);
        var typePropertyMethodModifier = Assert.Single(typePropertyDeclarationSyntax.Modifiers);
        Assert.Equal("public", typePropertyMethodModifier.Value);
        var predefinedTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(typePropertyDeclarationSyntax.Type);
        Assert.Equal("string", predefinedTypeSyntax.Keyword.Value);
    }
}
