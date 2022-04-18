using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateClientsFileTests
{
    [Fact]
    public void ImplementationFileNamespacesAndFileNameCorrect()
    {
        // Arrange
        var apiTestPathItem = new OpenApiPathItem
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                { OperationType.Get, new OpenApiOperation() },
            }
        };
        var spec = new OpenApiDocument()
        {
            Paths = new OpenApiPaths
            {
                { "/api/test", apiTestPathItem },
            },
        };

        // Act
        var writableFiles = FileGenerator.GenerateClients("MyNamespace", spec);

        // Assert
        var writableFile = Assert.Single(writableFiles);
        Assert.Equal("/GetApiTestClient.cs", writableFile.fileLocation);
        Assert.Equal("namespace", writableFile.namespaceDeclarationSyntax.NamespaceKeyword.Value);
        var namespaceIdentifier = Assert.IsType<QualifiedNameSyntax>(writableFile.namespaceDeclarationSyntax.Name);
        var leftNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Left);
        Assert.Equal("MyNamespace", leftNamespaceIdentifier.Identifier.Value);
        var rightNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Right);
        Assert.Equal("Clients", rightNamespaceIdentifier.Identifier.Value);
        var usingDirectiveSyntax = Assert.Single(writableFile.usingDirectiveSyntax!.Value);
        var usingName = Assert.IsType<IdentifierNameSyntax>(usingDirectiveSyntax.Name);
        Assert.Equal("MyNamespace.Models", usingName.Identifier.Value);
    }
}
