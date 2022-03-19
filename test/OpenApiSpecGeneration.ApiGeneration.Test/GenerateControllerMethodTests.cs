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
                parameters = new OpenApiMethodParameters[]
                {
                    new OpenApiMethodParameters
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
        Assert.Equal("testname", parameterSyntax.Identifier.ValueText);
        var parameterTypeSyntax = Assert.IsType<PredefinedTypeSyntax>(parameterSyntax.Type);
        Assert.Equal("int", parameterTypeSyntax.Keyword.Value);

        var parameterAttributeListSyntax = Assert.Single(parameterSyntax.AttributeLists);
        var parameterAttributeSyntax = Assert.Single(parameterAttributeListSyntax.Attributes);

        var parameterIdentifierNameSyntax = Assert.IsType<IdentifierNameSyntax>(parameterAttributeSyntax.Name);
        Assert.Equal("FromRoute", parameterIdentifierNameSyntax.Identifier.Value);
    }

    [Fact]
    public void InvalidParameterSchemaTypeThrows()
    {
        // Arrange
        var apiTestPath = new OpenApiPath
        {
            get = new OpenApiMethod
            {
                parameters = new OpenApiMethodParameters[]
                {
                    new OpenApiMethodParameters
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
                parameters = new OpenApiMethodParameters[]
                {
                    new OpenApiMethodParameters
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
