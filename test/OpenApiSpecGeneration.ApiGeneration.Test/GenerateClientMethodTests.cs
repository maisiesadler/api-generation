using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateClientMethodTests
{
    [Theory]
    [InlineData(OperationType.Get, "Get")]
    [InlineData(OperationType.Put, "Put")]
    [InlineData(OperationType.Post, "Post")]
    [InlineData(OperationType.Delete, "Delete")]
    public void HttpRequestMessageHasCorrectMethod(OperationType operationType, string expectedMethodType)
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(operationType);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateClients(document);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());

        var methodBodyStatement = methodDeclarationSyntax.Body!.Statements[0];

        var localDeclarationStatementSyntax = Assert.IsType<LocalDeclarationStatementSyntax>(methodDeclarationSyntax.Body!.Statements[0]);
        Assert.Equal(";", localDeclarationStatementSyntax.SemicolonToken.Value);
        var variableDeclarationSyntax = Assert.IsType<VariableDeclarationSyntax>(localDeclarationStatementSyntax.Declaration);
        var typeIdentifier = Assert.IsType<IdentifierNameSyntax>(variableDeclarationSyntax.Type);
        Assert.Equal("var", typeIdentifier.Identifier.Value);
        var variable = Assert.Single(variableDeclarationSyntax.Variables);
        Assert.Equal("request", variable.Identifier.Value);

        // EqualsValueClauseSyntax EqualsValueClause = new HttpRequestMessage{...}
        Assert.NotNull(variable.Initializer);
        Assert.Equal("=", variable.Initializer!.EqualsToken.Value);
        var objectCreationExpressionSyntax = Assert.IsType<ObjectCreationExpressionSyntax>(variable.Initializer!.Value);
        var initializerExpressionSyntax = Assert.IsType<InitializerExpressionSyntax>(objectCreationExpressionSyntax.Initializer);
        var expression = Assert.Single(objectCreationExpressionSyntax.Initializer?.Expressions);
        var assignmentExpressionSyntax = Assert.IsType<AssignmentExpressionSyntax>(expression);

        var left = Assert.IsType<IdentifierNameSyntax>(assignmentExpressionSyntax.Left);
        var right = Assert.IsType<MemberAccessExpressionSyntax>(assignmentExpressionSyntax.Right);
        Assert.Equal("Method", left.Identifier.Value);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(right.Expression);
        var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(right.Name);
        Assert.Equal("HttpMethod", identifierNameSyntax.Identifier.Value);
        Assert.Equal(expectedMethodType, methodIdentifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void MethodBodyCallsHttpClient()
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

        var methodBodyStatement = methodDeclarationSyntax.Body!.Statements[1];

        // var response = await _httpClient.SendAsync(request);
        var localDeclarationStatementSyntax = Assert.IsType<LocalDeclarationStatementSyntax>(methodBodyStatement);
        Assert.Equal(";", localDeclarationStatementSyntax.SemicolonToken.Value);
        var variableDeclarationSyntax = Assert.IsType<VariableDeclarationSyntax>(localDeclarationStatementSyntax.Declaration);
        var typeIdentifier = Assert.IsType<IdentifierNameSyntax>(variableDeclarationSyntax.Type);
        Assert.Equal("var", typeIdentifier.Identifier.Value);
        var variable = Assert.Single(variableDeclarationSyntax.Variables);
        // VariableDeclaratorSyntax VariableDeclarator response = await _httpClient.SendAsync(request)
        Assert.Equal("response", variable.Identifier.Value);

        // EqualsValueClauseSyntax EqualsValueClause = await _httpClient.SendAsync(request)
        Assert.NotNull(variable.Initializer);
        Assert.Equal("=", variable.Initializer!.EqualsToken.Value);
        var awaitExpressionSyntax = Assert.IsType<AwaitExpressionSyntax>(variable.Initializer!.Value);
        var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(awaitExpressionSyntax.Expression);
        var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Expression);
        var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Name);
        Assert.Equal("await", awaitExpressionSyntax.AwaitKeyword.Value);
        Assert.Equal("_httpClient", identifierNameSyntax.Identifier.Value);
        Assert.Equal("SendAsync", methodIdentifierNameSyntax.Identifier.Value);
        var argument = Assert.Single(invocationExpressionSyntax.ArgumentList.Arguments);
        var argumentIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(argument.Expression);
        Assert.Equal("request", argumentIdentifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void MethodBodyReadsContent()
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

        var methodBodyStatement = methodDeclarationSyntax.Body!.Statements[2];

        var localDeclarationStatementSyntax = Assert.IsType<LocalDeclarationStatementSyntax>(methodBodyStatement);
        Assert.Equal(";", localDeclarationStatementSyntax.SemicolonToken.Value);
        var variableDeclarationSyntax = Assert.IsType<VariableDeclarationSyntax>(localDeclarationStatementSyntax.Declaration);
        var typeIdentifier = Assert.IsType<IdentifierNameSyntax>(variableDeclarationSyntax.Type);
        Assert.Equal("var", typeIdentifier.Identifier.Value);
        var variable = Assert.Single(variableDeclarationSyntax.Variables);
        Assert.Equal("content", variable.Identifier.Value);

        Assert.NotNull(variable.Initializer);
        Assert.Equal("=", variable.Initializer!.EqualsToken.Value);
        var awaitExpressionSyntax = Assert.IsType<AwaitExpressionSyntax>(variable.Initializer!.Value);
        Assert.Equal("await", awaitExpressionSyntax.AwaitKeyword.Value);

        var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(awaitExpressionSyntax.Expression);
        Assert.Empty(invocationExpressionSyntax.ArgumentList.Arguments);

        // response.Content.ReadAsStringAsync
        var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);

        // response.Content
        var innerMemberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(memberAccessExpressionSyntax.Expression);
        var innerIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(innerMemberAccessExpressionSyntax.Expression);
        var innerMethodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(innerMemberAccessExpressionSyntax.Name);
        var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Name);
        Assert.Equal("response", innerIdentifierNameSyntax.Identifier.Value);
        Assert.Equal("Content", innerMethodIdentifierNameSyntax.Identifier.Value);
        Assert.Equal("ReadAsStringAsync", methodIdentifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void GenericClientResponseIfReturnType()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
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

        var methodBodyStatement = methodDeclarationSyntax.Body!.Statements[3];

        // return new ClientResponse<ToDoItem>(response.StatusCode, content);
        var returnStatementSyntax = Assert.IsType<ReturnStatementSyntax>(methodBodyStatement);
        var objectCreationExpressionSyntax = Assert.IsType<ObjectCreationExpressionSyntax>(returnStatementSyntax.Expression);
        Assert.Equal("new", objectCreationExpressionSyntax.NewKeyword.ValueText);

        // ClientResponse<ToDoItem>
        var genericNameSyntax = Assert.IsType<GenericNameSyntax>(objectCreationExpressionSyntax.Type);
        var genericIdentifierNameSyntax = Assert.IsType<SyntaxToken>(genericNameSyntax.Identifier);
        Assert.Equal("ClientResponse", genericIdentifierNameSyntax.Value);
        var typeArgument = Assert.Single(genericNameSyntax.TypeArgumentList.Arguments);
        var typeArgumentIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(typeArgument);
        Assert.Equal("ToDoItem", typeArgumentIdentifierNameSyntax.Identifier.Value);

        // response.StatusCode, content
        Assert.Equal(2, objectCreationExpressionSyntax.ArgumentList?.Arguments.Count);

        var firstArg = Assert.IsType<MemberAccessExpressionSyntax>(objectCreationExpressionSyntax.ArgumentList?.Arguments[0].Expression);
        Assert.Equal(SyntaxKind.SimpleMemberAccessExpression, firstArg.Kind());
        var firstArgIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(firstArg.Expression);
        Assert.Equal("response", firstArgIdentifierNameSyntax.Identifier.ValueText);
        Assert.Equal(".", firstArg.OperatorToken.Value);
        Assert.Equal("StatusCode", firstArg.Name.Identifier.Value);

        var secondArg = Assert.IsType<IdentifierNameSyntax>(objectCreationExpressionSyntax.ArgumentList?.Arguments[1].Expression);
        Assert.Equal("content", secondArg.Identifier.ValueText);
    }

    [Fact]
    public void NonGenericClientResponseIfNoReturnType()
    {
        // Arrange
        var response = OpenApiMockBuilder.BuildResponse("Success");

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

        var methodBodyStatement = methodDeclarationSyntax.Body!.Statements[3];

        // return new ClientResponse<ToDoItem>(response.StatusCode, content);
        var returnStatementSyntax = Assert.IsType<ReturnStatementSyntax>(methodBodyStatement);
        var objectCreationExpressionSyntax = Assert.IsType<ObjectCreationExpressionSyntax>(returnStatementSyntax.Expression);
        Assert.Equal("new", objectCreationExpressionSyntax.NewKeyword.ValueText);

        // ClientResponse
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(objectCreationExpressionSyntax.Type);
        Assert.Equal("ClientResponse", identifierNameSyntax.Identifier.Value);

        // response.StatusCode, content
        Assert.Equal(2, objectCreationExpressionSyntax.ArgumentList?.Arguments.Count);

        var firstArg = Assert.IsType<MemberAccessExpressionSyntax>(objectCreationExpressionSyntax.ArgumentList?.Arguments[0].Expression);
        Assert.Equal(SyntaxKind.SimpleMemberAccessExpression, firstArg.Kind());
        var firstArgIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(firstArg.Expression);
        Assert.Equal("response", firstArgIdentifierNameSyntax.Identifier.ValueText);
        Assert.Equal(".", firstArg.OperatorToken.Value);
        Assert.Equal("StatusCode", firstArg.Name.Identifier.Value);

        var secondArg = Assert.IsType<IdentifierNameSyntax>(objectCreationExpressionSyntax.ArgumentList?.Arguments[1].Expression);
        Assert.Equal("content", secondArg.Identifier.ValueText);
    }

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
