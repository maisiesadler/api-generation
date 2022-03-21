using Xunit;
using OpenApiSpecGeneration.Console;
using System.Threading.Tasks;
using OpenApiSpecGeneration.Console.Commands;

namespace OpenApiSpecGeneration.ApiGeneration.Console.Test;

public class GetOpenApiSpecFileTests
{
    [Fact]
    public async Task FileNotFoundReturnsFailure()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile();

        // Act
        var result = await getOpenApiSpecFile.Execute("/file/not/found");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task NestedComponentSchema_Loads()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile();
        var path = $"TestData/NestedComponentSchema.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.components.schemas);
        Assert.Equal("ToDoItem", key);
        Assert.Equal("object", value.type);

        var (propertyKey, propertyValue) = Assert.Single(value.properties);
        Assert.Equal("report", propertyKey);
        Assert.Equal("array", propertyValue.type);

        var (propertiesPropertyKey, propertiesPropertyValue) = Assert.Single(propertyValue.items?.properties);
        Assert.Equal("account_holders", propertiesPropertyKey);
        Assert.Equal("array", propertiesPropertyValue.type);

        var (propertiesPropertiesPropertyKey, propertiesPropertiesPropertyValue) = Assert.Single(propertiesPropertyValue.items?.properties);
        Assert.Equal("name", propertiesPropertiesPropertyKey);
        Assert.Equal("string", propertiesPropertiesPropertyValue.type);
    }

    [Fact]
    public async Task TodoGetWithPathParam_ResponsesLoadCorrectly()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile();
        var path = $"TestData/TodoGetWithPathParam.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.paths);
        Assert.Equal("/api/Todo/{id}", key);
        Assert.NotNull(value.get);
        Assert.Null(value.delete);
        Assert.Null(value.post);
        Assert.Null(value.put);

        var (responseKey, responseValue) = Assert.Single(value.get?.responses);
        Assert.Equal("200", responseKey);
        Assert.Equal("Success", responseValue.description);

        var (responseContentKey, responseContentValue) = Assert.Single(responseValue.content);
        Assert.Equal("text/plain", responseContentKey);
        Assert.Equal("array", responseContentValue.schema.type);

        Assert.Null(responseContentValue.schema.Ref);
        var (schemaItemKey, schemaItemValue) = Assert.Single(responseContentValue.schema.items);
        Assert.Equal("$ref", schemaItemKey);
        Assert.Equal("#/components/schemas/ToDoItem", schemaItemValue);
    }

    [Fact]
    public async Task TodoGetWithPathParam_ParametersLoadCorrectly()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile();
        var path = $"TestData/TodoGetWithPathParam.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.paths);
        Assert.Equal("/api/Todo/{id}", key);
        Assert.NotNull(value.get);
        Assert.Null(value.delete);
        Assert.Null(value.post);
        Assert.Null(value.put);

        var parameter = Assert.Single(value.get?.parameters);
        Assert.Equal("path", parameter.In);
        Assert.Equal("id", parameter.name);
        Assert.Equal("integer", parameter.schema?.type);
        Assert.Equal(1, parameter.schema?.minimum);
        Assert.Equal("The user ID", parameter.description);
    }

    [Fact]
    public async Task TodoGetWithQueryParam_ParametersLoadCorrectly()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile();
        var path = $"TestData/TodoGetWithQueryParam.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.paths);
        Assert.Equal("/api/Todo", key);
        Assert.NotNull(value.get);
        Assert.Null(value.delete);
        Assert.Null(value.post);
        Assert.Null(value.put);

        var parameter = Assert.Single(value.get?.parameters);
        Assert.Equal("query", parameter.In);
        Assert.Equal("offset", parameter.name);
        Assert.Equal("string", parameter.schema?.type);
        Assert.Equal("The number of items to skip before starting to collect the result set", parameter.description);
    }

    [Fact]
    public async Task TodoGetWithHeaderParam_ParametersLoadCorrectly()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile();
        var path = $"TestData/TodoGetWithHeaderParam.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.paths);
        Assert.Equal("/api/Todo", key);
        Assert.NotNull(value.get);
        Assert.Null(value.delete);
        Assert.Null(value.post);
        Assert.Null(value.put);

        var parameter = Assert.Single(value.get?.parameters);
        Assert.Equal("header", parameter.In);
        Assert.Equal("requestId", parameter.name);
        Assert.Equal("string", parameter.schema?.type);
    }
}
