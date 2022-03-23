using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateControllersTests
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
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        Assert.Equal("ApiTest", classDeclarationSyntax.Identifier.Value);
        var classModifier = Assert.Single(classDeclarationSyntax.Modifiers);
        Assert.Equal("public", classModifier.Value);

        Assert.NotNull(classDeclarationSyntax.BaseList);
        Assert.Equal(":", classDeclarationSyntax.BaseList!.ColonToken.Value);
        var baseType = Assert.Single(classDeclarationSyntax.BaseList.Types);
        var baseTypeIdentifier = Assert.IsType<IdentifierNameSyntax>(baseType.Type);
        Assert.Equal("ControllerBase", baseTypeIdentifier.Identifier.Value);
    }

    [Fact]
    public void MethodSignatureIsCorrect()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
           .WithOperation(OperationType.Get);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

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
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
           .WithOperation(
               OperationType.Get,
               operation => operation.Tags.Add(new OpenApiTag { Name = "peas" }));

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem)
            .WithPath("/api/test/{id}", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document).ToList();

        // Assert
        Assert.Equal(2, classDeclarationSyntaxes.Count);

        Assert.Equal("ApiTest", classDeclarationSyntaxes[0].Identifier.Value);
        Assert.Equal("ApiTestId", classDeclarationSyntaxes[1].Identifier.Value);
    }

    [Fact]
    public void ClassHasPublicConstructor()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
           .WithOperation(
               OperationType.Get,
               operation => operation.Tags.Add(new OpenApiTag { Name = "peas" }));

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

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
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

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
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

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
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

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
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
           .WithOperation(OperationType.Get);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

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
    [InlineData(OperationType.Get, "Get", "HttpGet")]
    [InlineData(OperationType.Post, "Post", "HttpPost")]
    [InlineData(OperationType.Put, "Put", "HttpPut")]
    [InlineData(OperationType.Delete, "Delete", "HttpDelete")]
    public void MethodHasRouteAttribute(OperationType operationType, string expectedMethodName, string expectedRouteAttribute)
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
           .WithOperation(operationType);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

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
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());

        Assert.Equal(2, methodDeclarationSyntax.Body!.Statements.Count);

        // var result = await _interactor.Execute();
        var localDeclarationStatementSyntax = Assert.IsType<LocalDeclarationStatementSyntax>(methodDeclarationSyntax.Body!.Statements[0]);
        var variableDeclarationSyntax = Assert.IsType<VariableDeclarationSyntax>(localDeclarationStatementSyntax.Declaration);
        var typeIdentifier = Assert.IsType<IdentifierNameSyntax>(variableDeclarationSyntax.Type);
        Assert.Equal("var", typeIdentifier.Identifier.Value);
        var variable = Assert.Single(variableDeclarationSyntax.Variables);
        // VariableDeclaratorSyntax VariableDeclarator result = await _getApiTestInteractor.Execute()
        Assert.Equal("result", variable.Identifier.Value);

        // EqualsValueClauseSyntax EqualsValueClause = await _getApiTestInteractor.Execute()
        Assert.NotNull(variable.Initializer);
        Assert.Equal("=", variable.Initializer!.EqualsToken.Value);
        var awaitExpressionSyntax = Assert.IsType<AwaitExpressionSyntax>(variable.Initializer!.Value);
        var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(awaitExpressionSyntax.Expression);
        var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Expression);
        var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Name);
        Assert.Equal("await", awaitExpressionSyntax.AwaitKeyword.Value);
        Assert.Equal("_getApiTestInteractor", identifierNameSyntax.Identifier.Value);
        Assert.Equal("Execute", methodIdentifierNameSyntax.Identifier.Value);
        Assert.Equal(";", localDeclarationStatementSyntax.SemicolonToken.Value);

        // return Ok(result);
        var returnStatementSyntax = Assert.IsType<ReturnStatementSyntax>(methodDeclarationSyntax.Body!.Statements[1]);

        // InvocationExpressionSyntax InvocationExpression Ok(result)
        var returnOkInvocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(returnStatementSyntax.Expression);
        var returnOkInvocationIdentifier = Assert.IsType<IdentifierNameSyntax>(returnOkInvocationExpressionSyntax.Expression);
        Assert.Equal("Ok", returnOkInvocationIdentifier.Identifier.Value);
        var singleArgument = Assert.Single(returnOkInvocationExpressionSyntax.ArgumentList.Arguments);
        var singleArgumentIdentifier = Assert.IsType<IdentifierNameSyntax>(singleArgument.Expression);
        Assert.Equal("result", singleArgumentIdentifier.Identifier.Value);
        Assert.Equal(";", returnStatementSyntax.SemicolonToken.Value);
    }

    [Fact]
    public void ResultNotSetIfReturnTypeIsVoid()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
           .WithOperation(OperationType.Get);

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

        // Assert
        var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
        var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());

        Assert.Equal(2, methodDeclarationSyntax.Body!.Statements.Count);

        // await _interactor.Execute();
        var expressionStatementSyntax = Assert.IsType<ExpressionStatementSyntax>(methodDeclarationSyntax.Body!.Statements[0]);
        var awaitExpressionSyntax = Assert.IsType<AwaitExpressionSyntax>(expressionStatementSyntax.Expression);
        var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(awaitExpressionSyntax.Expression);
        var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Expression);
        var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Name);
        Assert.Equal("await", awaitExpressionSyntax.AwaitKeyword.Value);
        Assert.Equal("_getApiTestInteractor", identifierNameSyntax.Identifier.Value);
        Assert.Equal("Execute", methodIdentifierNameSyntax.Identifier.Value);
        Assert.Equal(";", expressionStatementSyntax.SemicolonToken.Value);

        // return Ok(result);
        var returnStatementSyntax = Assert.IsType<ReturnStatementSyntax>(methodDeclarationSyntax.Body!.Statements[1]);

        // InvocationExpressionSyntax InvocationExpression Ok(result)
        var returnOkInvocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(returnStatementSyntax.Expression);
        var returnOkInvocationIdentifier = Assert.IsType<IdentifierNameSyntax>(returnOkInvocationExpressionSyntax.Expression);
        Assert.Equal("Ok", returnOkInvocationIdentifier.Identifier.Value);
        Assert.Empty(returnOkInvocationExpressionSyntax.ArgumentList.Arguments);
        Assert.Equal(";", returnStatementSyntax.SemicolonToken.Value);
    }
}
