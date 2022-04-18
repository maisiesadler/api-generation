using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateClientMethodTests
{
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

        var methodBodyStatement = Assert.Single(methodDeclarationSyntax.Body!.Statements);

        // var response = await _httpClient.SendAsync(request);
        var localDeclarationStatementSyntax = Assert.IsType<LocalDeclarationStatementSyntax>(methodDeclarationSyntax.Body!.Statements[0]);
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
