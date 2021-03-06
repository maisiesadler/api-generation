using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateInteractorsFileTests
{
    [Fact]
    public void InteractorFileNamespacesAndFileNameCorrect()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema(),
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
        var writableFiles = FileGenerator.GenerateInteractors("MyNamespace", document);

        // Assert
        var writableFile = Assert.Single(writableFiles);
        Assert.Equal("/interactors/IGetApiTestInteractor.cs", writableFile.fileLocation);
        Assert.Equal("namespace", writableFile.namespaceDeclarationSyntax.NamespaceKeyword.Value);
        var namespaceIdentifier = Assert.IsType<QualifiedNameSyntax>(writableFile.namespaceDeclarationSyntax.Name);
        var leftNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Left);
        Assert.Equal("MyNamespace", leftNamespaceIdentifier.Identifier.Value);
        var rightNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Right);
        Assert.Equal("Interactors", rightNamespaceIdentifier.Identifier.Value);
        var usingDirective = Assert.Single(writableFile.usingDirectiveSyntax!.Value);
        var usingName = Assert.IsType<IdentifierNameSyntax>(usingDirective.Name);
        Assert.Equal("MyNamespace.Models", usingName.Identifier.Value);
    }
}
