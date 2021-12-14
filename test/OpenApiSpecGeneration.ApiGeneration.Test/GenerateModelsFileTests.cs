using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.Entities;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateModelsFileTests
{
    [Fact]
    public void ModelFileNamespacesAndFileNameCorrect()
    {
        // Arrange
        var toDoItemProperties = new Dictionary<string, OpenApiComponentProperty>
        {
            { "id", new OpenApiComponentProperty("integer", default, default, default) },
        };
        var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
        {
            { "ToDoItem", new OpenApiComponentSchema("object", toDoItemProperties) }
        };
        var components = new OpenApiComponent(componentSchemas);
        var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

        // Act
        var writableFiles = FileGenerator.GenerateModels("MyNamespace", spec);

        // Assert
        var writableFile = Assert.Single(writableFiles);
        Assert.Equal("/models/ToDoItem.cs", writableFile.fileLocation);
        Assert.Equal("namespace", writableFile.namespaceDeclarationSyntax.NamespaceKeyword.Value);
        var namespaceIdentifier = Assert.IsType<QualifiedNameSyntax>(writableFile.namespaceDeclarationSyntax.Name);
        var leftNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Left);
        Assert.Equal("MyNamespace", leftNamespaceIdentifier.Identifier.Value);
        var rightNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Right);
        Assert.Equal("Models", rightNamespaceIdentifier.Identifier.Value);
        var singleUsing = Assert.Single(writableFile.usingDirectiveSyntax!.Value);
        var usingName0 = Assert.IsType<IdentifierNameSyntax>(singleUsing.Name);
        Assert.Equal("System.Text.Json.Serialization", usingName0.Identifier.Value);
    }
}
