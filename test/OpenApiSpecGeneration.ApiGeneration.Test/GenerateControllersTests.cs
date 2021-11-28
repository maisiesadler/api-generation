using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateControllersTests
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
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        Assert.Equal("ApiTest", classDeclarationSyntax.Identifier.Value);
        var classModifier = Assert.Single(classDeclarationSyntax.Modifiers);
        Assert.Equal("public", classModifier.Value);

        Assert.Equal(2, classDeclarationSyntax.Members.Count);
        var memberDeclarationSyntax = classDeclarationSyntax.Members[1]; // first is ctor, skip here
        var methodDeclarationSyntax = Assert.IsType<MethodDeclarationSyntax>(memberDeclarationSyntax);
        Assert.Equal("Get", methodDeclarationSyntax.Identifier.Value);
        var methodModifier = Assert.Single(methodDeclarationSyntax.Modifiers);
        Assert.Equal("public", methodModifier.Value);
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
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

        // Assert
        Assert.Equal(2, classDeclarationSyntaxes.Count);

        Assert.Equal("ApiTest", classDeclarationSyntaxes[0].Identifier.Value);
        Assert.Equal("ApiTestId", classDeclarationSyntaxes[1].Identifier.Value);
    }

    [Fact]
    public void ClassHasPublicConstructor()
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
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var memberDeclarationSyntax = classDeclarationSyntax.Members.First();
        var methodDeclarationSyntax = Assert.IsType<ConstructorDeclarationSyntax>(memberDeclarationSyntax);
        Assert.Equal("ApiTest", methodDeclarationSyntax.Identifier.Value);
        var methodModifier = Assert.Single(methodDeclarationSyntax.Modifiers);
        Assert.Equal("public", methodModifier.Value);

        // parameter list
    }
}
