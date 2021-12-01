using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateControllersTests
{
    [Fact]
    public void ClassSignatureIsCorrect()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            { "get", new OpenApiMethod {} },
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
    }

    [Fact]
    public void MethodSignatureIsCorrect()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            { "get", new OpenApiMethod {} },
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

        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());
        Assert.Equal("Get", methodDeclarationSyntax.Identifier.Value);
        Assert.Equal(2, methodDeclarationSyntax.Modifiers.Count);
        Assert.Equal("public", methodDeclarationSyntax.Modifiers[0].Value);
        Assert.Equal("async", methodDeclarationSyntax.Modifiers[1].Value);

        var genericNameSyntax = Assert.IsType<GenericNameSyntax>(methodDeclarationSyntax.ReturnType);
        Assert.Equal("Task", genericNameSyntax.Identifier.Value);
        var argument = Assert.Single(genericNameSyntax.TypeArgumentList.Arguments);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(argument);
        Assert.Equal("IActionResult", identifierNameSyntax.Identifier.Value);
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
        var constructorDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<ConstructorDeclarationSyntax>());
        Assert.Equal("ApiTest", constructorDeclarationSyntax.Identifier.Value);
        var methodModifier = Assert.Single(constructorDeclarationSyntax.Modifiers);
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
        var constructorDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<ConstructorDeclarationSyntax>());
        var ctorParameter = Assert.Single(constructorDeclarationSyntax.ParameterList.Parameters);
        Assert.Equal("getApiTestInteractor", ctorParameter.Identifier.Value);
        var parameterType = Assert.IsType<IdentifierNameSyntax>(ctorParameter.Type);
        Assert.Equal("IGetApiTestInteractor", parameterType.Identifier.Value);
    }

    [Fact]
    public void PublicConstructorHasInteractorsAsPrivateFields()
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
        var fieldDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<FieldDeclarationSyntax>());
        Assert.Equal(2, fieldDeclarationSyntax.Modifiers.Count);
        Assert.Equal("private", fieldDeclarationSyntax.Modifiers[0].Value);
        Assert.Equal("readonly", fieldDeclarationSyntax.Modifiers[1].Value);
        var declarationType = Assert.IsType<IdentifierNameSyntax>(fieldDeclarationSyntax.Declaration.Type);
        Assert.Equal("IGetApiTestInteractor", declarationType.Identifier.Value);
        var variable = Assert.Single(fieldDeclarationSyntax.Declaration.Variables);
        Assert.Equal("_getApiTestInteractor", variable.Identifier.Value);
        Assert.Equal(";", fieldDeclarationSyntax.SemicolonToken.Value);
    }

    [Fact]
    public void PublicConstructorHasInteractorsSetsPrivateFields()
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
        var constructorDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<ConstructorDeclarationSyntax>());
        var statementSyntax = Assert.Single(constructorDeclarationSyntax.Body!.Statements);
        var expressionStatementSyntax = Assert.IsType<ExpressionStatementSyntax>(statementSyntax);
        var assignmentExpressionSyntax = Assert.IsType<AssignmentExpressionSyntax>(expressionStatementSyntax.Expression);
        var left = Assert.IsType<IdentifierNameSyntax>(assignmentExpressionSyntax.Left);
        var right = Assert.IsType<IdentifierNameSyntax>(assignmentExpressionSyntax.Right);
        Assert.Equal("_getApiTestInteractor", left.Identifier.Value);
        Assert.Equal("getApiTestInteractor", right.Identifier.Value);
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
        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());
        Assert.Equal(expectedMethodName, methodDeclarationSyntax.Identifier.Value);

        var methodAttributeListSyntax = Assert.Single(methodDeclarationSyntax.AttributeLists);

        var methodAttributeSyntax = Assert.Single(methodAttributeListSyntax.Attributes);
        var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(methodAttributeSyntax.Name);
        Assert.Equal(expectedRouteAttribute, methodIdentifierNameSyntax.Identifier.Value);
        Assert.Null(methodAttributeSyntax.ArgumentList);
    }

    [Fact]
    public void MethodBodyCallsInteractor()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            { "get", new OpenApiMethod {} },
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
        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());
        var statementSyntax = Assert.Single(methodDeclarationSyntax.Body!.Statements);

        var returnStatementSyntax = Assert.IsType<ReturnStatementSyntax>(statementSyntax);
        var awaitExpressionSyntax = Assert.IsType<AwaitExpressionSyntax>(returnStatementSyntax.Expression);
        Assert.Equal("return", returnStatementSyntax.ReturnKeyword.Value);
        var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(awaitExpressionSyntax.Expression);
        var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Expression);
        var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Name);
        Assert.Equal("_getApiTestInteractor", identifierNameSyntax.Identifier.Value);
        Assert.Equal("Execute", methodIdentifierNameSyntax.Identifier.Value);
        Assert.Equal(";", returnStatementSyntax.SemicolonToken.Value);
    }
}
