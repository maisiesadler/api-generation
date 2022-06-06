using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture.Test;

public class GenerateImplementationFileTests
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

        var definition = Definition.DefinitionGenerator.GenerateDefinition(spec);

        // Act
        var writableFiles = AutoFixture.FileGenerator.GenerateImplementation("MyNamespace", definition);

        // Assert
        var writableFile = Assert.Single(writableFiles);
        Assert.Equal("/implementations/GetApiTestInteractor.cs", writableFile.fileLocation);
        Assert.Equal("namespace", writableFile.namespaceDeclarationSyntax.NamespaceKeyword.Value);
        var namespaceIdentifier = Assert.IsType<QualifiedNameSyntax>(writableFile.namespaceDeclarationSyntax.Name);
        var leftNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Left);
        Assert.Equal("MyNamespace", leftNamespaceIdentifier.Identifier.Value);
        var rightNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Right);
        Assert.Equal("GeneratedImplementations", rightNamespaceIdentifier.Identifier.Value);
        Assert.Equal(3, writableFile.usingDirectiveSyntax!.Value.Count);
        var usingName0 = Assert.IsType<IdentifierNameSyntax>(writableFile.usingDirectiveSyntax!.Value[0].Name);
        var usingName1 = Assert.IsType<IdentifierNameSyntax>(writableFile.usingDirectiveSyntax!.Value[1].Name);
        var usingName2 = Assert.IsType<IdentifierNameSyntax>(writableFile.usingDirectiveSyntax!.Value[2].Name);
        Assert.Equal("AutoFixture", usingName0.Identifier.Value);
        Assert.Equal("MyNamespace.Interactors", usingName1.Identifier.Value);
        Assert.Equal("MyNamespace.Models", usingName2.Identifier.Value);
    }
}
