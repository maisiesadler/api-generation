using Xunit;
using System.Threading.Tasks;
using OpenApiSpecGeneration.Console.Commands.Helpers;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.ApiGeneration.Console.Test;

public class GetOpenApiSpecFilePathTests
{
    [Fact]
    public async Task TodoGetWithPathParam_ResponsesLoadCorrectly()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile2();
        var path = $"TestData/TodoGetWithPathParam.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.Paths);
        Assert.Equal("/api/Todo/{id}", key);

        var (operationType, operation) = Assert.Single(value.Operations);
        Assert.Equal(OperationType.Get, operationType);

        var (responseKey, responseValue) = Assert.Single(operation.Responses);
        Assert.Equal("200", responseKey);
        Assert.Equal("Success", responseValue.Description);

        var (responseContentKey, responseContentValue) = Assert.Single(responseValue.Content);
        Assert.Equal("text/plain", responseContentKey);
        Assert.Equal("array", responseContentValue.Schema.Type);

        Assert.Null(responseContentValue.Schema.Reference);
        Assert.Equal("ToDoItem", responseContentValue.Schema.Items.Reference.Id);
        Assert.Equal("#/components/schemas/ToDoItem", responseContentValue.Schema.Items.Reference.ReferenceV3);
        Assert.True(responseContentValue.Schema.Items.Reference.IsLocal);
    }

    [Fact]
    public async Task TodoGetWithPathParam_ParametersLoadCorrectly()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile2();
        var path = $"TestData/TodoGetWithPathParam.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.Paths);
        Assert.Equal("/api/Todo/{id}", key);

        var (operationType, operation) = Assert.Single(value.Operations);
        Assert.Equal(OperationType.Get, operationType);

        var parameter = Assert.Single(operation.Parameters);
        Assert.Equal(ParameterLocation.Path, parameter.In);
        Assert.Equal("id", parameter.Name);
        Assert.Equal("integer", parameter.Schema?.Type);
        Assert.Equal(1, parameter.Schema?.Minimum);
        Assert.Equal("The user ID", parameter.Description);
    }

    [Fact]
    public async Task TodoGetWithQueryParam_ParametersLoadCorrectly()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile2();
        var path = $"TestData/TodoGetWithQueryParam.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.Paths);
        Assert.Equal("/api/Todo", key);

        var (operationType, operation) = Assert.Single(value.Operations);
        Assert.Equal(OperationType.Get, operationType);

        var parameter = Assert.Single(operation.Parameters);
        Assert.Equal(ParameterLocation.Query, parameter.In);
        Assert.Equal("offset", parameter.Name);
        Assert.Equal("string", parameter.Schema?.Type);
        Assert.Equal("The number of items to skip before starting to collect the result set", parameter.Description);
    }

    [Fact]
    public async Task TodoGetWithHeaderParam_ParametersLoadCorrectly()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile2();
        var path = $"TestData/TodoGetWithHeaderParam.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.Paths);
        Assert.Equal("/api/Todo", key);
        var (operationType, operation) = Assert.Single(value.Operations);

        Assert.Equal(OperationType.Get, operationType);

        var parameter = Assert.Single(operation.Parameters);
        Assert.Equal(ParameterLocation.Header, parameter.In);
        Assert.Equal("requestId", parameter.Name);
        Assert.Equal("string", parameter.Schema?.Type);
    }
}
