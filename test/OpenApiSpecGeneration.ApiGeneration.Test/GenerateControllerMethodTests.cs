using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateControllerMethodTests
{
    [Fact]
    public void ValidMethodPathParameterGenerated()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            get = new OpenApiMethod
            {
                parameters = new OpenApiMethodParameter[]
                {
                    new OpenApiMethodParameter
                    {
                        In = "path",
                        name = "testname",
                        required = true,
                        description = "something",
                        schema = new OpenApiMethodParameterSchema("integer", null)
                    },
                },
            },
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

        var parameterSyntax = Assert.Single(methodDeclarationSyntax.ParameterList.Parameters);
        Assert.Equal("testname", parameterSyntax.Identifier.Value);
        var parameterSyntaxType = Assert.IsType<PredefinedTypeSyntax>(parameterSyntax.Type);
        Assert.Equal("int", parameterSyntaxType.Keyword.Value);

        var parameterAttributeListSyntax = Assert.Single(parameterSyntax.AttributeLists);
        var parameterAttributeSyntax = Assert.Single(parameterAttributeListSyntax.Attributes);

        var parameterIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(parameterAttributeSyntax.Name);
        Assert.Equal("FromRoute", parameterIdentifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void ValidMethodPathParameterPassedToInteractor()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            get = new OpenApiMethod
            {
                parameters = new OpenApiMethodParameter[]
                {
                    new OpenApiMethodParameter
                    {
                        In = "path",
                        name = "testname",
                        required = true,
                        description = "something",
                        schema = new OpenApiMethodParameterSchema("integer", null)
                    },
                },
            },
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

        Assert.Equal(2, methodDeclarationSyntax.Body!.Statements.Count);

        // await _interactor.Execute(int testname);
        var expressionStatementSyntax = Assert.IsType<ExpressionStatementSyntax>(methodDeclarationSyntax.Body!.Statements[0]);

        var awaitExpressionSyntax = Assert.IsType<AwaitExpressionSyntax>(expressionStatementSyntax.Expression);
        var invocationExpressionSyntax = Assert.IsType<InvocationExpressionSyntax>(awaitExpressionSyntax.Expression);
        var memberAccessExpressionSyntax = Assert.IsType<MemberAccessExpressionSyntax>(invocationExpressionSyntax.Expression);
        var methodIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(memberAccessExpressionSyntax.Name);

        Assert.Equal("Execute", methodIdentifierNameSyntax.Identifier.Value);

        var argumentSyntax = Assert.Single(invocationExpressionSyntax.ArgumentList.Arguments);
        var argumentSyntaxExpression = Assert.IsType<IdentifierNameSyntax>(argumentSyntax.Expression);
        Assert.Equal("testname", argumentSyntaxExpression.Identifier.Value);
    }

    [Fact]
    public void InvalidParameterSchemaTypeThrows()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            get = new OpenApiMethod
            {
                parameters = new OpenApiMethodParameter[]
                {
                    new OpenApiMethodParameter
                    {
                        In = "path",
                        name = "testname",
                        required = true,
                        description = "something",
                        schema = new OpenApiMethodParameterSchema("potatoes", null)
                    },
                },
            },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => ApiGenerator.GenerateControllers(spec).ToList());

        Assert.Equal($"Unknown openapi type 'potatoes'", exception.Message);
    }

    [Fact]
    public void InvalidParameterTypeThrows()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            get = new OpenApiMethod
            {
                parameters = new OpenApiMethodParameter[]
                {
                    new OpenApiMethodParameter
                    {
                        In = "potatoes",
                        name = "testname",
                        required = true,
                        description = "something",
                        schema = new OpenApiMethodParameterSchema("integer", null)
                    },
                },
            },
        };
        var paths = new Dictionary<string, OpenApiPath>
        {
            { "/api/test", apiTestPath },
        };
        var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => ApiGenerator.GenerateControllers(spec).ToList());

        Assert.Equal($"Unknown parameter type 'potatoes'", exception.Message);
    }
}
