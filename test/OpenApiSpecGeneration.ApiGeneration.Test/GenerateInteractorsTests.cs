using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateInteractorsTests
{
    [Fact]
    public void ValidInteractorCreatedForOnePath()
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
            get =new OpenApiMethod { responses = openApiResponses },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var interfaceDeclarationSyntaxes = ApiGenerator.GenerateInteractors(spec);

        // Assert
        var interfaceDeclarationSyntax = Assert.Single(interfaceDeclarationSyntaxes);
        Assert.Equal("IGetApiTestInteractor", interfaceDeclarationSyntax.Identifier.Value);
        var classModifier = Assert.Single(interfaceDeclarationSyntax.Modifiers);
        Assert.Equal("public", classModifier.Value);

        Assert.Equal("{", interfaceDeclarationSyntax.OpenBraceToken.Value);
        Assert.Equal("}", interfaceDeclarationSyntax.CloseBraceToken.Value);
        var memberDeclarationSyntax = Assert.Single(interfaceDeclarationSyntax.Members);
        var methodDeclarationSyntax = Assert.IsType<MethodDeclarationSyntax>(memberDeclarationSyntax);
        Assert.Equal("Execute", methodDeclarationSyntax.Identifier.Value);
        Assert.Empty(methodDeclarationSyntax.Modifiers);
        var genericNameSyntax = Assert.IsType<GenericNameSyntax>(methodDeclarationSyntax.ReturnType);
        Assert.Equal("Task", genericNameSyntax.Identifier.Value);
        var typeArgument = Assert.Single(genericNameSyntax.TypeArgumentList.Arguments);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(typeArgument);
        Assert.Equal("ToDoItem", identifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void NoReturnTypeSetTask()
    {
        // Arrange
        var openApiContentSchema = new OpenApiContentSchema("array", new Dictionary<string, string>());
        var openApiResponse = new OpenApiResponse("Success", new Dictionary<string, OpenApiContent>
        {
            { "text/plain", new OpenApiContent(openApiContentSchema) }
        });
        var openApiResponses = new Dictionary<string, OpenApiResponse> { { "200", openApiResponse } };
        var apiTestPath = new OpenApiPath
        {
            get = new OpenApiMethod { responses = openApiResponses },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var interfaceDeclarationSyntaxes = ApiGenerator.GenerateInteractors(spec);

        // Assert
        var interfaceDeclarationSyntax = Assert.Single(interfaceDeclarationSyntaxes);
        Assert.Equal("IGetApiTestInteractor", interfaceDeclarationSyntax.Identifier.Value);
        var classModifier = Assert.Single(interfaceDeclarationSyntax.Modifiers);
        Assert.Equal("public", classModifier.Value);

        Assert.Equal("{", interfaceDeclarationSyntax.OpenBraceToken.Value);
        Assert.Equal("}", interfaceDeclarationSyntax.CloseBraceToken.Value);
        var memberDeclarationSyntax = Assert.Single(interfaceDeclarationSyntax.Members);
        var methodDeclarationSyntax = Assert.IsType<MethodDeclarationSyntax>(memberDeclarationSyntax);
        Assert.Equal("Execute", methodDeclarationSyntax.Identifier.Value);
        Assert.Empty(methodDeclarationSyntax.Modifiers);
        var genericNameSyntax = Assert.IsType<IdentifierNameSyntax>(methodDeclarationSyntax.ReturnType);
        Assert.Equal("Task", genericNameSyntax.Identifier.Value);
    }
}
