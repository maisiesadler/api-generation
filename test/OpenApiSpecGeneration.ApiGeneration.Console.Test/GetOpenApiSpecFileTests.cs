using Xunit;
using OpenApiSpecGeneration.Console;
using System.Threading.Tasks;

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
}
