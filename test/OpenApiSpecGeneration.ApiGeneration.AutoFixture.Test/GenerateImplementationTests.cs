using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.AutoFixture.Test;

public class GenerateImplementationTests
{
    [Fact]
    public void ClassSignatureIsCorrect()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            get = new OpenApiMethod { },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateImplementations(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        Assert.Equal("GetApiTestInteractor", classDeclarationSyntax.Identifier.Value);
        var classModifier = Assert.Single(classDeclarationSyntax.Modifiers);
        Assert.Equal("public", classModifier.Value);

        Assert.NotNull(classDeclarationSyntax.BaseList);
        Assert.Equal(":", classDeclarationSyntax.BaseList!.ColonToken.Value);
        var baseType = Assert.Single(classDeclarationSyntax.BaseList.Types);
        var baseTypeIdentifier = Assert.IsType<IdentifierNameSyntax>(baseType.Type);
        Assert.Equal("IGetApiTestInteractor", baseTypeIdentifier.Identifier.Value);
    }

    [Fact]
    public void NoReturnTypeMethodSignatureIsCorrect()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            get = new OpenApiMethod { },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateImplementations(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);

        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());
        Assert.Equal("Execute", methodDeclarationSyntax.Identifier.Value);
        Assert.Equal(2, methodDeclarationSyntax.Modifiers.Count);
        Assert.Equal("public", methodDeclarationSyntax.Modifiers[0].Value);
        Assert.Equal("async", methodDeclarationSyntax.Modifiers[1].Value);

        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(methodDeclarationSyntax.ReturnType);
        Assert.Equal("Task", identifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void ReturnTypeMethodSignatureIsCorrect()
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
            get = new OpenApiMethod { responses = openApiResponses },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateImplementations(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);

        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());
        Assert.Equal("Execute", methodDeclarationSyntax.Identifier.Value);
        Assert.Equal(2, methodDeclarationSyntax.Modifiers.Count);
        Assert.Equal("public", methodDeclarationSyntax.Modifiers[0].Value);
        Assert.Equal("async", methodDeclarationSyntax.Modifiers[1].Value);

        var genericNameSyntax = Assert.IsType<GenericNameSyntax>(methodDeclarationSyntax.ReturnType);
        Assert.Equal("Task", genericNameSyntax.Identifier.Value);
        var argument = Assert.Single(genericNameSyntax.TypeArgumentList.Arguments);
        var arrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(argument);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(arrayTypeSyntax.ElementType);
        Assert.Equal("ToDoItem", identifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void MethodBodyGeneratesReturnType()
    {
        // Arrange
        var openApiContentSchema = new OpenApiContentSchema("object", new Dictionary<string, string>())
        {
            Ref = "#/components/schemas/ToDoItem",
        };
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
        var classDeclarationSyntaxes = ApiGenerator.GenerateImplementations(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());

        var methodBodyStatement = Assert.Single(methodDeclarationSyntax.Body!.Statements);

        // return _fixture.Create<ToDoItem>();
        var returnStatementSyntax = Assert.IsType<ReturnStatementSyntax>(methodBodyStatement);
        var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(returnStatementSyntax.Expression);
        var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);

        // _fixture
        var memberAccessIdentifierSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Expression);
        Assert.Equal("_fixture", memberAccessIdentifierSyntax.Identifier.Value);

        // Create<ToDoItem>
        var genericNameSyntax = Assert.IsType<GenericNameSyntax>(methodDeclarationSyntax.ReturnType);
        Assert.Equal("Task", genericNameSyntax.Identifier.Value);
        var argument = Assert.Single(genericNameSyntax.TypeArgumentList.Arguments);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(argument);
        Assert.Equal("ToDoItem", identifierNameSyntax.Identifier.Value);
        var memberAccessMethodGenericSyntax = Assert.IsType<GenericNameSyntax>(memberAccessExpressionSyntax.Name);
        Assert.Equal("Create", memberAccessMethodGenericSyntax.Identifier.Value);

        // ();
        Assert.Empty(invocationExpressionSyntax.ArgumentList.Arguments);
        Assert.Equal(";", returnStatementSyntax.SemicolonToken.Value);
    }

    [Fact]
    public void MethodBodyEmptyIfNoReturnType()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            get = new OpenApiMethod { },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateImplementations(spec);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());

        Assert.Empty(methodDeclarationSyntax.Body!.Statements);
    }
}
