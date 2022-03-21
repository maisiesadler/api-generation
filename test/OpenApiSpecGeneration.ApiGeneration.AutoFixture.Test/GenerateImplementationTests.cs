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

    // [Fact]
    // public void TwoPathsGeneratedAsSeparateFiles()
    // {
    //     // Arrange
    //     var apiTestPath = new OpenApiPath
    //     {
    //         get = new OpenApiMethod { tags = new List<string> { "peas" } },
    //     };
    //     var paths = new Dictionary<string, OpenApiPath>
    //     {
    //         { "/api/test", apiTestPath },
    //         { "/api/test/{id}", apiTestPath },
    //     };
    //     var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

    //     // Act
    //     var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec).ToList();

    //     // Assert
    //     Assert.Equal(2, classDeclarationSyntaxes.Count);

    //     Assert.Equal("ApiTest", classDeclarationSyntaxes[0].Identifier.Value);
    //     Assert.Equal("ApiTestId", classDeclarationSyntaxes[1].Identifier.Value);
    // }
    // [Fact]
    // public void ControllerHasApiControllerAttribute()
    // {
    //     // Arrange
    //     var apiTestPath = new OpenApiPath
    //     {
    //         get = new OpenApiMethod { tags = new List<string> { "peas" } },
    //     };
    //     var paths = new Dictionary<string, OpenApiPath>
    //     {
    //         { "/api/test", apiTestPath },
    //     };
    //     var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

    //     // Act
    //     var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

    //     // Assert
    //     var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);

    //     Assert.Equal(2, classDeclarationSyntax.AttributeLists.Count);

    //     var controllerAttributeListSyntax = classDeclarationSyntax.AttributeLists[0];
    //     var controllerAttributeSyntax = Assert.Single(controllerAttributeListSyntax.Attributes);
    //     var controllerIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(controllerAttributeSyntax.Name);
    //     Assert.Equal("ApiController", controllerIdentifierNameSyntax.Identifier.Value);
    //     Assert.Null(controllerAttributeSyntax.ArgumentList);

    //     var routeAttributeListSyntax = classDeclarationSyntax.AttributeLists[1];
    //     var routeAttributeSyntax = Assert.Single(routeAttributeListSyntax.Attributes);
    //     var routeIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(routeAttributeSyntax.Name);
    //     Assert.Equal("Route", routeIdentifierNameSyntax.Identifier.Value);

    //     Assert.NotNull(routeAttributeSyntax.ArgumentList);
    //     Assert.Equal("(", routeAttributeSyntax.ArgumentList!.OpenParenToken.Value);
    //     Assert.Equal(")", routeAttributeSyntax.ArgumentList!.CloseParenToken.Value);
    //     var routeAttributeArgument = Assert.Single(routeAttributeSyntax.ArgumentList!.Arguments);
    //     var literalExpressionSyntax = Assert.IsType<LiteralExpressionSyntax>(routeAttributeArgument.Expression);
    //     Assert.Equal("\"/api/test\"", literalExpressionSyntax.Token.Value);
    // }

    // [Fact]
    // public void MethodBodyCallsInteractor()
    // {
    //     // Arrange
    //     var openApiContentSchema = new OpenApiContentSchema("array", new Dictionary<string, string>
    //     {
    //         { "$ref", "#/components/schemas/ToDoItem" },
    //     });
    //     var openApiResponse = new OpenApiResponse("Success", new Dictionary<string, OpenApiContent>
    //     {
    //         { "text/plain", new OpenApiContent(openApiContentSchema) }
    //     });
    //     var openApiResponses = new Dictionary<string, OpenApiResponse> { { "200", openApiResponse } };
    //     var apiTestPath = new OpenApiPath
    //     {
    //         get = new OpenApiMethod { responses = openApiResponses },
    //     };
    //     var paths = new Dictionary<string, OpenApiPath>
    //     {
    //         { "/api/test", apiTestPath },
    //     };
    //     var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

    //     // Act
    //     var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

    //     // Assert
    //     var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
    //     var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());

    //     Assert.Equal(2, methodDeclarationSyntax.Body!.Statements.Count);

    //     // var result = await _interactor.Execute();
    //     var localDeclarationStatementSyntax = Assert.IsType<LocalDeclarationStatementSyntax>(methodDeclarationSyntax.Body!.Statements[0]);
    //     var variableDeclarationSyntax = Assert.IsType<VariableDeclarationSyntax>(localDeclarationStatementSyntax.Declaration);
    //     var typeIdentifier = Assert.IsType<IdentifierNameSyntax>(variableDeclarationSyntax.Type);
    //     Assert.Equal("var", typeIdentifier.Identifier.Value);
    //     var variable = Assert.Single(variableDeclarationSyntax.Variables);
    //     // VariableDeclaratorSyntax VariableDeclarator result = await _getApiTestInteractor.Execute()
    //     Assert.Equal("result", variable.Identifier.Value);

    //     // EqualsValueClauseSyntax EqualsValueClause = await _getApiTestInteractor.Execute()
    //     Assert.NotNull(variable.Initializer);
    //     Assert.Equal("=", variable.Initializer!.EqualsToken.Value);
    //     var awaitExpressionSyntax = Assert.IsType<AwaitExpressionSyntax>(variable.Initializer!.Value);
    //     var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(awaitExpressionSyntax.Expression);
    //     var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);
    //     var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Expression);
    //     var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Name);
    //     Assert.Equal("await", awaitExpressionSyntax.AwaitKeyword.Value);
    //     Assert.Equal("_getApiTestInteractor", identifierNameSyntax.Identifier.Value);
    //     Assert.Equal("Execute", methodIdentifierNameSyntax.Identifier.Value);
    //     Assert.Equal(";", localDeclarationStatementSyntax.SemicolonToken.Value);

    //     // return Ok(result);
    //     var returnStatementSyntax = Assert.IsType<ReturnStatementSyntax>(methodDeclarationSyntax.Body!.Statements[1]);

    //     // InvocationExpressionSyntax InvocationExpression Ok(result)
    //     var returnOkInvocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(returnStatementSyntax.Expression);
    //     var returnOkInvocationIdentifier = Assert.IsType<IdentifierNameSyntax>(returnOkInvocationExpressionSyntax.Expression);
    //     Assert.Equal("Ok", returnOkInvocationIdentifier.Identifier.Value);
    //     var singleArgument = Assert.Single(returnOkInvocationExpressionSyntax.ArgumentList.Arguments);
    //     var singleArgumentIdentifier = Assert.IsType<IdentifierNameSyntax>(singleArgument.Expression);
    //     Assert.Equal("result", singleArgumentIdentifier.Identifier.Value);
    //     Assert.Equal(";", returnStatementSyntax.SemicolonToken.Value);
    // }

    // [Fact]
    // public void ResultNotSetIfReturnTypeIsVoid()
    // {
    //     // Arrange
    //     var apiTestPath = new OpenApiPath
    //     {
    //         get = new OpenApiMethod { },
    //     };
    //     var paths = new Dictionary<string, OpenApiPath>
    //     {
    //         { "/api/test", apiTestPath },
    //     };
    //     var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

    //     // Act
    //     var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(spec);

    //     // Assert
    //     var classDeclarationSyntax = Assert.Single(classDeclarationSyntaxes);
    //     var methodDeclarationSyntax = Assert.Single(classDeclarationSyntax.Members.GetMembersOfType<MethodDeclarationSyntax>());

    //     Assert.Equal(2, methodDeclarationSyntax.Body!.Statements.Count);

    //     // await _interactor.Execute();
    //     var expressionStatementSyntax = Assert.IsType<ExpressionStatementSyntax>(methodDeclarationSyntax.Body!.Statements[0]);
    //     var awaitExpressionSyntax = Assert.IsType<AwaitExpressionSyntax>(expressionStatementSyntax.Expression);
    //     var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(awaitExpressionSyntax.Expression);
    //     var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);
    //     var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Expression);
    //     var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Name);
    //     Assert.Equal("await", awaitExpressionSyntax.AwaitKeyword.Value);
    //     Assert.Equal("_getApiTestInteractor", identifierNameSyntax.Identifier.Value);
    //     Assert.Equal("Execute", methodIdentifierNameSyntax.Identifier.Value);
    //     Assert.Equal(";", expressionStatementSyntax.SemicolonToken.Value);

    //     // return Ok(result);
    //     var returnStatementSyntax = Assert.IsType<ReturnStatementSyntax>(methodDeclarationSyntax.Body!.Statements[1]);

    //     // InvocationExpressionSyntax InvocationExpression Ok(result)
    //     var returnOkInvocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(returnStatementSyntax.Expression);
    //     var returnOkInvocationIdentifier = Assert.IsType<IdentifierNameSyntax>(returnOkInvocationExpressionSyntax.Expression);
    //     Assert.Equal("Ok", returnOkInvocationIdentifier.Identifier.Value);
    //     Assert.Empty(returnOkInvocationExpressionSyntax.ArgumentList.Arguments);
    //     Assert.Equal(";", returnStatementSyntax.SemicolonToken.Value);
    // }
}
