using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateClientsTests
{
    [Fact]
    public void ClassSignatureIsCorrect()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(OperationType.Get);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateClients(document);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        Assert.Equal("GetApiTestClient", classDeclarationSyntax.Identifier.Value);
        var classModifier = Assert.Single(classDeclarationSyntax.Modifiers);
        Assert.Equal("public", classModifier.Value);

        Assert.Null(classDeclarationSyntax.BaseList);
    }

    [Fact]
    public void HttpClientAsPrivateFields()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(OperationType.Get);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateClients(document);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var fieldDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<FieldDeclarationSyntax>());
        Assert.Equal(2, fieldDeclarationSyntax.Modifiers.Count);
        Assert.Equal("private", fieldDeclarationSyntax.Modifiers[0].Value);
        Assert.Equal("readonly", fieldDeclarationSyntax.Modifiers[1].Value);
        var declarationType = Assert.IsType<IdentifierNameSyntax>(fieldDeclarationSyntax.Declaration.Type);
        Assert.Equal("HttpClient", declarationType.Identifier.Value);
        var variable = Assert.Single(fieldDeclarationSyntax.Declaration.Variables);
        Assert.Equal("_httpClient", variable.Identifier.Value);
        Assert.Equal(";", fieldDeclarationSyntax.SemicolonToken.Value);
    }


    [Fact]
    public void PublicConstructorHasHttpClientInjected()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(OperationType.Get);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateClients(document);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var constructorDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<ConstructorDeclarationSyntax>());
        var ctorParameter = Assert.Single(constructorDeclarationSyntax.ParameterList.Parameters);
        Assert.Equal("httpClient", ctorParameter.Identifier.Value);
        var parameterType = Assert.IsType<IdentifierNameSyntax>(ctorParameter.Type);
        Assert.Equal("HttpClient", parameterType.Identifier.Value);
    }

    [Fact]
    public void PublicConstructorSetsHttpClient()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(OperationType.Get);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateClients(document);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var constructorDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<ConstructorDeclarationSyntax>());
        var statementSyntax = Assert.Single(constructorDeclarationSyntax.Body!.Statements);
        var expressionStatementSyntax = Assert.IsType<ExpressionStatementSyntax>(statementSyntax);
        var assignmentExpressionSyntax = Assert.IsType<AssignmentExpressionSyntax>(expressionStatementSyntax.Expression);
        var left = Assert.IsType<IdentifierNameSyntax>(assignmentExpressionSyntax.Left);
        var right = Assert.IsType<IdentifierNameSyntax>(assignmentExpressionSyntax.Right);
        Assert.Equal("_httpClient", left.Identifier.Value);
        Assert.Equal("httpClient", right.Identifier.Value);
    }

    [Fact]
    public void NoReturnTypeMethodSignatureIsCorrect()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(OperationType.Get);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateClients(document);

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
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(argument);
        Assert.Equal("ClientResponse", identifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void ReturnTypeMethodSignatureIsCorrect()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema
            {
                Reference = new OpenApiReference { Id = "ToDoItem", },
            },
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
        var classDeclarationSyntaxes = ApiGenerator.GenerateClients(document);

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
        var genericNameSyntax0 = Assert.IsType<GenericNameSyntax>(argument);
        Assert.Equal("ClientResponse", genericNameSyntax0.Identifier.Value);
        var argument0 = Assert.Single(genericNameSyntax0.TypeArgumentList.Arguments);
        var arrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(argument0);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(arrayTypeSyntax.ElementType);
        Assert.Equal("ToDoItem", identifierNameSyntax.Identifier.Value);
    }

    // [Fact]
    // public void ImplementationHasFixtureAsPrivateField()
    // {
    //     // Arrange
    //     var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
    //         .WithOperation(OperationType.Get);

    //     var document = OpenApiMockBuilder.BuildDocument()
    //         .WithPath("/api/test", apiTestPathItem);

    //     // Act
    //     var classDeclarationSyntaxes = ApiGenerator.GenerateImplementations(document);

    //     // Assert
    //     var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
    //     var fieldDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<FieldDeclarationSyntax>());
    //     Assert.Equal(2, fieldDeclarationSyntax.Modifiers.Count);
    //     Assert.Equal("private", fieldDeclarationSyntax.Modifiers[0].Value);
    //     Assert.Equal("readonly", fieldDeclarationSyntax.Modifiers[1].Value);
    //     var declarationType = Assert.IsType<IdentifierNameSyntax>(fieldDeclarationSyntax.Declaration.Type);
    //     Assert.Equal("Fixture", declarationType.Identifier.Value);
    //     var variable = Assert.Single(fieldDeclarationSyntax.Declaration.Variables);
    //     Assert.Equal("_fixture", variable.Identifier.Value);
    //     Assert.Equal(";", fieldDeclarationSyntax.SemicolonToken.Value);

    //     var equalsValueClauseSyntax = Assert.IsType<EqualsValueClauseSyntax>(variable.Initializer);
    //     var objectCreationExpressionSyntax = Assert.IsType<ObjectCreationExpressionSyntax>(equalsValueClauseSyntax.Value);
    //     Assert.Empty(objectCreationExpressionSyntax.ArgumentList?.Arguments);
    //     var identifierName = Assert.IsType<IdentifierNameSyntax>(objectCreationExpressionSyntax.Type);
    //     Assert.Equal("Fixture", identifierName.Identifier.ValueText);
    //     Assert.Equal("new", objectCreationExpressionSyntax.NewKeyword.ValueText);
    // }

    // [Fact]
    // public void MethodBodyGeneratesReturnType()
    // {
    //     // Arrange
    //     var responseSchema = new OpenApiSchema
    //     {
    //         Type = "object",
    //         Items = new OpenApiSchema
    //         {
    //             Reference = new OpenApiReference { Id = "ToDoItem", },
    //         },
    //     };

    //     var response = OpenApiMockBuilder.BuildResponse("Success")
    //          .AddContent("text/plain", responseSchema);

    //     var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
    //         .WithOperation(
    //             OperationType.Get,
    //             operation => operation.Responses.Add("200", response)
    //         );

    //     var document = OpenApiMockBuilder.BuildDocument()
    //         .WithPath("/api/test", apiTestPathItem);

    //     // Act
    //     var classDeclarationSyntaxes = ApiGenerator.GenerateImplementations(document);

    //     // Assert
    //     var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
    //     var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());

    //     var methodBodyStatement = Assert.Single(methodDeclarationSyntax.Body!.Statements);

    //     // return _fixture.Create<ToDoItem>();
    //     var returnStatementSyntax = Assert.IsType<ReturnStatementSyntax>(methodBodyStatement);
    //     var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(returnStatementSyntax.Expression);
    //     var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);

    //     // _fixture
    //     var memberAccessIdentifierSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Expression);
    //     Assert.Equal("_fixture", memberAccessIdentifierSyntax.Identifier.Value);

    //     // Create<ToDoItem>
    //     var genericNameSyntax = Assert.IsType<GenericNameSyntax>(methodDeclarationSyntax.ReturnType);
    //     Assert.Equal("Task", genericNameSyntax.Identifier.Value);
    //     var argument = Assert.Single(genericNameSyntax.TypeArgumentList.Arguments);
    //     var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(argument);
    //     Assert.Equal("ToDoItem", identifierNameSyntax.Identifier.Value);
    //     var memberAccessMethodGenericSyntax = Assert.IsType<GenericNameSyntax>(memberAccessExpressionSyntax.Name);
    //     Assert.Equal("Create", memberAccessMethodGenericSyntax.Identifier.Value);

    //     // ();
    //     Assert.Empty(invocationExpressionSyntax.ArgumentList.Arguments);
    //     Assert.Equal(";", returnStatementSyntax.SemicolonToken.Value);
    // }

    // [Fact]
    // public void MethodBodyEmptyIfNoReturnType()
    // {
    //     // Arrange
    //     var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
    //         .WithOperation(OperationType.Get);

    //     var document = OpenApiMockBuilder.BuildDocument()
    //         .WithPath("/api/test", apiTestPathItem);

    //     // Act
    //     var classDeclarationSyntaxes = ApiGenerator.GenerateImplementations(document);

    //     // Assert
    //     var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
    //     var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());

    //     Assert.Empty(methodDeclarationSyntax.Body!.Statements);
    // }

    // [Fact]
    // public void RequestBodyMethodSignatureIsCorrect()
    // {
    //     // Arrange
    //     var requestBodySchema = new OpenApiSchema
    //     {
    //         Type = "object",
    //         Properties = new Dictionary<string, OpenApiSchema>
    //         {
    //             { "id", new OpenApiSchema { Type = "string" }},
    //         }
    //     };

    //     var requestBody = OpenApiMockBuilder.BuildRequestBody("This is the request")
    //          .AddContent("text/plain", requestBodySchema);

    //     var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
    //         .WithOperation(
    //             OperationType.Post,
    //             operation => operation.RequestBody = requestBody
    //         );

    //     var document = OpenApiMockBuilder.BuildDocument()
    //         .WithPath("/api/test", apiTestPathItem);

    //     // Act
    //     var classDeclarationSyntaxes = ApiGenerator.GenerateImplementations(document);

    //     // Assert
    //     var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);

    //     var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());
    //     Assert.Equal("Execute", methodDeclarationSyntax.Identifier.Value);
    //     Assert.Equal(2, methodDeclarationSyntax.Modifiers.Count);
    //     Assert.Equal("public", methodDeclarationSyntax.Modifiers[0].Value);
    //     Assert.Equal("async", methodDeclarationSyntax.Modifiers[1].Value);

    //     var parameterSyntax = Assert.Single(methodDeclarationSyntax.ParameterList.Parameters);
    //     Assert.Equal("request", parameterSyntax.Identifier.Value);
    //     var parameterSyntaxType = Assert.IsType<IdentifierNameSyntax>(parameterSyntax.Type);
    //     Assert.Equal("ApiTestPostTextPlainRequest", parameterSyntaxType.Identifier.Value);

    //     Assert.Empty(parameterSyntax.AttributeLists);
    // }
}
