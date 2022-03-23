using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.ApiGeneration.OpenApiMocks;
using Xunit;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public class GenerateInteractorsTests
{
    [Fact]
    public void ValidInteractorCreatedForOnePath()
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
        var interfaceDeclarationSyntaxes = ApiGenerator.GenerateInteractors(document);

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
        var arrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(typeArgument);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(arrayTypeSyntax.ElementType);
        Assert.Equal("ToDoItem", identifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void SetReturnTypeAsArray()
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
        var interfaceDeclarationSyntaxes = ApiGenerator.GenerateInteractors(document);

        // Assert
        var interfaceDeclarationSyntax = Assert.Single(interfaceDeclarationSyntaxes);
        var memberDeclarationSyntax = Assert.Single(interfaceDeclarationSyntax.Members);
        var methodDeclarationSyntax = Assert.IsType<MethodDeclarationSyntax>(memberDeclarationSyntax);

        var genericNameSyntax = Assert.IsType<GenericNameSyntax>(methodDeclarationSyntax.ReturnType);
        Assert.Equal("Task", genericNameSyntax.Identifier.Value);
        var typeArgument = Assert.Single(genericNameSyntax.TypeArgumentList.Arguments);
        var arrayTypeSyntax = Assert.IsType<ArrayTypeSyntax>(typeArgument);
        var identifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(arrayTypeSyntax.ElementType);
        Assert.Equal("ToDoItem", identifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void ReturnTypeSchemaRefDirectlyInSchema()
    {
        // Arrange
        var responseSchema = new OpenApiSchema
        {
            Type = "object",
            Reference = new OpenApiReference { Id = "ToDoItem", },
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
        var interfaceDeclarationSyntaxes = ApiGenerator.GenerateInteractors(document);

        // Assert
        var interfaceDeclarationSyntax = Assert.Single(interfaceDeclarationSyntaxes);

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
        var responseSchema = new OpenApiSchema
        {
            Type = "array",
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
        var interfaceDeclarationSyntaxes = ApiGenerator.GenerateInteractors(document);

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

    [Theory]
    [InlineData("test", "test")]
    [InlineData("x-request-id", "xRequestId")]
    public void ParametersSetIfAvailable(string paramName, string expectedGeneratedParam)
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
        var interfaceDeclarationSyntaxes = ApiGenerator.GenerateInteractors(document);

        // Assert
        var interfaceDeclarationSyntax = Assert.Single(interfaceDeclarationSyntaxes);
        Assert.Equal("IGetApiTestInteractor", interfaceDeclarationSyntax.Identifier.Value);

        var memberDeclarationSyntax = Assert.Single(interfaceDeclarationSyntax.Members);
        var methodDeclarationSyntax = Assert.IsType<MethodDeclarationSyntax>(memberDeclarationSyntax);

        Assert.Equal("Execute", methodDeclarationSyntax.Identifier.Value);
        var parameterSyntax = Assert.Single(methodDeclarationSyntax.ParameterList.Parameters);

        Assert.Equal(expectedGeneratedParam, parameterSyntax.Identifier.ValueText);
        var parameterTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(parameterSyntax.Type);
        Assert.Equal("int", parameterTypeSyntax.Keyword.Value);
    }
}
