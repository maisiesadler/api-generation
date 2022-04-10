using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateModelsTestDataNestedComponentSchemaTests
{
    [Fact]
    public async Task GeneratedModelNamesCorrect()
    {
        // Arrange
        var document = await TestDataCache.Get("NestedComponentSchema");

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document).ToArray();

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
    public async Task RootModelPropertiesCorrect()
    {
        // Arrange
        var document = await TestDataCache.Get("NestedComponentSchema");

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document).ToArray();

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
        var nullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(typePropertyDeclarationSyntax.Type);
        var typePropertyArrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(nullableTypeSyntax.ElementType);
        var typePropertyArrayElementType = Assert.IsType<IdentifierNameSyntax>(typePropertyArrayTypeSyntax.ElementType);
        Assert.Equal("ToDoItemReportSubType", typePropertyArrayElementType.Identifier.Value);
    }

    [Fact]
    public async Task NestedModelPropertiesCorrect()
    {
        // Arrange
        var document = await TestDataCache.Get("NestedComponentSchema");

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document).ToArray();

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
        var nullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(typePropertyDeclarationSyntax.Type);
        var typePropertyArrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(nullableTypeSyntax.ElementType);
        var typePropertyArrayElementType = Assert.IsType<IdentifierNameSyntax>(typePropertyArrayTypeSyntax.ElementType);
        Assert.Equal("ToDoItemReportSubTypeAccountHoldersSubType", typePropertyArrayElementType.Identifier.Value);
    }

    [Fact]
    public async Task NestedNestedModelPropertiesCorrect()
    {
        // Arrange
        var document = await TestDataCache.Get("NestedComponentSchema");

        // Act
        var recordDeclarationSyntaxes = ApiGenerator.GenerateModels(document).ToArray();

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
        var nullableTypeSyntax = Assert.IsType<NullableTypeSyntax>(typePropertyDeclarationSyntax.Type);
        var predefinedTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(nullableTypeSyntax.ElementType);
        Assert.Equal("string", predefinedTypeSyntax.Keyword.Value);
    }
}
