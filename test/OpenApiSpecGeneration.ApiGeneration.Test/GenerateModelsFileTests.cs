using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateModelsFileTests
{
    [Fact]
    public void ModelFileNamespacesAndFileNameCorrect()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "id", new OpenApiSchema { Type ="integer" } },
                },
            },
        };

        var document = OpenApiMockBuilder.BuildDocument()
            .WithComponentSchema("ToDoItem", responseSchema);

        var definition = Definition.DefinitionGenerator.GenerateDefinition(document);

        // Act
        var writableFiles = FileGenerator.GenerateModels("MyNamespace", definition);

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
