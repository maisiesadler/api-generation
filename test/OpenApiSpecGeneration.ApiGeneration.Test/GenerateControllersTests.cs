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
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

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
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

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
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var memberDeclarationSyntax = classDeclarationSyntax.Members.First();
        var methodDeclarationSyntax = Assert.IsType<ConstructorDeclarationSyntax>(memberDeclarationSyntax);
        Assert.Equal("ApiTest", methodDeclarationSyntax.Identifier.Value);
        var methodModifier = Assert.Single(methodDeclarationSyntax.Modifiers);
        Assert.Equal("public", methodModifier.Value);
    }

    [Fact]
    public void PublicConstructorHasInteractorAsParameter()
    {
        // Arrange
        var openApiContentSchema = new OpenApiContentSchema("array", new Dictionary<string, string>
        {
            { "$ref", "#/components/schemas/ToDoItem" },
        });
        var openApiResponse = new OpenApiResponse("Success", new Dictionary<string, OpenApiContent>
        {
            { "text/plain", new OpenApiContent(openApiContentSchema) }
        });
        var openApiResponses = new Dictionary<string, OpenApiResponse> { { "200", openApiResponse } };
        var apiTestPath = new OpenApiPath
        {
            { "get", new OpenApiMethod { responses = openApiResponses } },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var memberDeclarationSyntax = classDeclarationSyntax.Members.First();
        var methodDeclarationSyntax = Assert.IsType<ConstructorDeclarationSyntax>(memberDeclarationSyntax);
        var ctorParameter = Assert.Single(methodDeclarationSyntax.ParameterList.Parameters);
        Assert.Equal("getApiTestInteractor", ctorParameter.Identifier.Value);
        var parameterType = Assert.IsType<IdentifierNameSyntax>(ctorParameter.Type);
        Assert.Equal("IGetApiTestInteractor", parameterType.Identifier.Value);
    }

    [Fact]
    public void ControllerHasApiControllerAttribute()
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
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);

        Assert.Equal(2, classDeclarationSyntax.AttributeLists.Count);

        var controllerAttributeListSyntax = classDeclarationSyntax.AttributeLists[0];
        var controllerAttributeSyntax = Assert.Single(controllerAttributeListSyntax.Attributes);
        var controllerIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(controllerAttributeSyntax.Name);
        Assert.Equal("ApiController", controllerIdentifierNameSyntax.Identifier.Value);
        Assert.Null(controllerAttributeSyntax.ArgumentList);

        var routeAttributeListSyntax = classDeclarationSyntax.AttributeLists[1];
        var routeAttributeSyntax = Assert.Single(routeAttributeListSyntax.Attributes);
        var routeIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(routeAttributeSyntax.Name);
        Assert.Equal("Route", routeIdentifierNameSyntax.Identifier.Value);

        Assert.NotNull(routeAttributeSyntax.ArgumentList);
        Assert.Equal("(", routeAttributeSyntax.ArgumentList!.OpenParenToken.Value);
        Assert.Equal(")", routeAttributeSyntax.ArgumentList!.CloseParenToken.Value);
        var routeAttributeArgument = Assert.Single(routeAttributeSyntax.ArgumentList!.Arguments);
        var literalExpressionSyntax = Assert.IsType<LiteralExpressionSyntax>(routeAttributeArgument.Expression);
        Assert.Equal("\"/api/test\"", literalExpressionSyntax.Token.Value);
    }

    [Theory]
    [InlineData("get", "Get", "HttpGet")]
    [InlineData("post", "Post", "HttpPost")]
    [InlineData("put", "Put", "HttpPut")]
    [InlineData("delete", "Delete", "HttpDelete")]
    public void MethodHasRouteAttribute(string method, string expectedMethodName, string expectedRouteAttribute)
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            { method, new OpenApiMethod {} },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        Assert.Equal(2, classDeclarationSyntax.Members.Count);
        var memberDeclarationSyntax = classDeclarationSyntax.Members[1];
        var methodDeclarationSyntax = Assert.IsType<MethodDeclarationSyntax>(memberDeclarationSyntax);
        Assert.Equal(expectedMethodName, methodDeclarationSyntax.Identifier.Value);

        var methodAttributeListSyntax = Assert.Single(methodDeclarationSyntax.AttributeLists);

        var methodAttributeSyntax = Assert.Single(methodAttributeListSyntax.Attributes);
        var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(methodAttributeSyntax.Name);
        Assert.Equal(expectedRouteAttribute, methodIdentifierNameSyntax.Identifier.Value);
        Assert.Null(methodAttributeSyntax.ArgumentList);
    }
}
