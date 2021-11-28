using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class ApiGeneratorTests
{
    [Fact]
    public void SimpleGetIsGeneratedAsMethod()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            { "get", new OpenApiMethod { tags = new List<string> { "peas" } } },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.Generate(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        Assert.Equal("ApiTest", classDeclarationSyntax.Identifier.Value);

        var memberDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members);
        var methodDeclarationSyntax = Assert.IsType<MethodDeclarationSyntax>(memberDeclarationSyntax);
        Assert.Equal("Get", methodDeclarationSyntax.Identifier.Value);
        var modifier = Assert.Single(methodDeclarationSyntax.Modifiers);
        Assert.Equal("public", modifier.Value);
    }

    [Fact]
    public void TwoPathsGeneratedAsSeparateFiles()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            { "get", new OpenApiMethod { tags = new List<string> { "peas" } } },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
            { "/api/test/{id}", apiTestPath },
        };
        var spec = new OpenApiSpec(paths);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.Generate(spec);

        // Assert
        Assert.Equal(2, classDeclarationSyntaxes.Count);

        Assert.Equal("ApiTest", classDeclarationSyntaxes[0].Identifier.Value);
        Assert.Equal("ApiTestId", classDeclarationSyntaxes[1].Identifier.Value);
    }
}
