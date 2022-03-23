using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateControllerFileTests
{
    [Fact]
    public void ControllerFileNamespacesAndFileNameCorrect()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
           .WithOperation(OperationType.Get);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var writableFiles = FileGenerator.GenerateControllers("MyNamespace", document);

        // Assert
        var writableFile = Assert.Single(writableFiles);
        Assert.Equal("ApiTestController.cs", writableFile.fileLocation);
        Assert.Equal("namespace", writableFile.namespaceDeclarationSyntax.NamespaceKeyword.Value);
        var namespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(writableFile.namespaceDeclarationSyntax.Name);
        Assert.Equal("MyNamespace", namespaceIdentifier.Identifier.Value);
        Assert.Equal(3, writableFile.usingDirectiveSyntax!.Value.Count);
        var usingName0 = Assert.IsType<IdentifierNameSyntax>(writableFile.usingDirectiveSyntax!.Value[0].Name);
        var usingName1 = Assert.IsType<IdentifierNameSyntax>(writableFile.usingDirectiveSyntax!.Value[1].Name);
        var usingName2 = Assert.IsType<IdentifierNameSyntax>(writableFile.usingDirectiveSyntax!.Value[2].Name);
        Assert.Equal("Microsoft.AspNetCore.Mvc", usingName0.Identifier.Value);
        Assert.Equal("MyNamespace.Interactors", usingName1.Identifier.Value);
        Assert.Equal("MyNamespace.Models", usingName2.Identifier.Value);
    }
}
