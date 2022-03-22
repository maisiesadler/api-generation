using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateControllerMethodTests
{
    [Fact]
    public void ValidMethodPathParameterGenerated()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(
                OperationType.Get,
                operation => operation.Parameters.Add(new OpenApiParameter
                {
                    In = ParameterLocation.Path,
                    Name = "testname",
                    Required = true,
                    Description = "something",
                    Schema = new OpenApiSchema { Type = "integer" },
                }));

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

        var parameterSyntax = Assert.Single(methodDeclarationSyntax.ParameterList.Parameters);
        Assert.Equal("testname", parameterSyntax.Identifier.Value);
        var parameterSyntaxType = Assert.IsType<PredefinedTypeSyntax>(parameterSyntax.Type);
        Assert.Equal("int", parameterSyntaxType.Keyword.Value);

        var parameterAttributeListSyntax = Assert.Single(parameterSyntax.AttributeLists);
        var parameterAttributeSyntax = Assert.Single(parameterAttributeListSyntax.Attributes);

        var parameterIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(parameterAttributeSyntax.Name);
        Assert.Equal("FromRoute", parameterIdentifierNameSyntax.Identifier.Value);
        Assert.Null(parameterAttributeSyntax.ArgumentList);
    }

    [Fact]
    public void ValidMethodQueryParameterGenerated()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(
                OperationType.Get,
                operation => operation.Parameters.Add(new OpenApiParameter
                {
                    In = ParameterLocation.Query,
                    Name = "queryParam",
                    Required = true,
                    Description = "something",
                    Schema = new OpenApiSchema { Type = "string" },
                }));

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

        var parameterSyntax = Assert.Single(methodDeclarationSyntax.ParameterList.Parameters);
        Assert.Equal("queryParam", parameterSyntax.Identifier.Value);
        var parameterSyntaxType = Assert.IsType<PredefinedTypeSyntax>(parameterSyntax.Type);
        Assert.Equal("string", parameterSyntaxType.Keyword.Value);

        var parameterAttributeListSyntax = Assert.Single(parameterSyntax.AttributeLists);
        var parameterAttributeSyntax = Assert.Single(parameterAttributeListSyntax.Attributes);

        var parameterIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(parameterAttributeSyntax.Name);
        Assert.Equal("FromQuery", parameterIdentifierNameSyntax.Identifier.Value);
        Assert.Null(parameterAttributeSyntax.ArgumentList);
    }

    [Fact]
    public void ValidMethodHeaderParameterGenerated()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(
                OperationType.Get,
                operation => operation.Parameters.Add(new OpenApiParameter
                {
                    In = ParameterLocation.Header,
                    Name = "x-request-id",
                    Required = true,
                    Description = "something",
                    Schema = new OpenApiSchema { Type = "string" },
                }));

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

        var parameterSyntax = Assert.Single(methodDeclarationSyntax.ParameterList.Parameters);
        Assert.Equal("xRequestId", parameterSyntax.Identifier.Value);
        var parameterSyntaxType = Assert.IsType<PredefinedTypeSyntax>(parameterSyntax.Type);
        Assert.Equal("string", parameterSyntaxType.Keyword.Value);

        var parameterAttributeListSyntax = Assert.Single(parameterSyntax.AttributeLists);
        var parameterAttributeSyntax = Assert.Single(parameterAttributeListSyntax.Attributes);

        var parameterIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(parameterAttributeSyntax.Name);
        Assert.Equal("FromHeader", parameterIdentifierNameSyntax.Identifier.Value);

        var singleAttributeArgument = Assert.Single(parameterAttributeSyntax.ArgumentList?.Arguments);
        var attributeArgument = Assert.IsType<AttributeArgumentSyntax>(singleAttributeArgument);
        Assert.IsType<NameEqualsSyntax>(attributeArgument.NameEquals);

        var expression = Assert.IsType<LiteralExpressionSyntax>(attributeArgument.Expression);
        Assert.Equal("\"x-request-id\"", expression.Token.Text);
    }

    [Theory]
    [InlineData("paramName", "paramName")]
    [InlineData("x-request-id", "xRequestId")]
    public void ValidMethodParameterPassedToInteractor(string paramName, string expectedGeneratedParam)
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(
                OperationType.Get,
                operation => operation.Parameters.Add(new OpenApiParameter
                {
                    In = ParameterLocation.Path,
                    Name = paramName,
                    Required = true,
                    Description = "something",
                    Schema = new OpenApiSchema { Type = "integer" },
                }));

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var classDeclarationSyntaxes = ApiGenerator.GenerateControllers(document);

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
        Assert.Equal(expectedGeneratedParam, argumentSyntaxExpression.Identifier.Value);
    }

    [Fact]
    public void InvalidParameterSchemaTypeThrows()
    {
        // Arrange
        var apiTestPathItem = OpenApiMockBuilder.BuildPathItem()
            .WithOperation(
                OperationType.Get,
                operation => operation.Parameters.Add(new OpenApiParameter
                {
                    In = ParameterLocation.Path,
                    Name = "testname",
                    Required = true,
                    Description = "something",
                    Schema = new OpenApiSchema { Type = "potatoes" },
                }));

        var document = OpenApiMockBuilder.BuildDocument()
            .WithPath("/api/test", apiTestPathItem);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => ApiGenerator.GenerateControllers(document).ToList());

        Assert.Equal($"Unknown openapi type 'potatoes'", exception.Message);
    }

    // [Fact]
    // public void InvalidParameterTypeThrows()
    // {
    //     // Arrange
    //     var apiTestPath = new OpenApiPath
    //     {
    //         get = new OpenApiMethod
    //         {
    //             parameters = new OpenApiMethodParameter[]
    //             {
    //                 new OpenApiMethodParameter
    //                 {
    //                     In = "potatoes",
    //                     name = "testname",
    //                     required = true,
    //                     description = "something",
    //                     schema = new OpenApiMethodParameterSchema("integer", null)
    //                 },
    //             },
    //         },
    //     };
    //     var paths = new Dictionary<string, OpenApiPath>
    //     {
    //         { "/api/test", apiTestPath },
    //     };
    //     var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

    //     // Act & Assert
    //     var exception = Assert.Throws<InvalidOperationException>(() => ApiGenerator.GenerateControllers(spec).ToList());

    //     Assert.Equal($"Unknown parameter type 'potatoes'", exception.Message);
    // }
}
