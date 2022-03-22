using Xunit;
using System.Threading.Tasks;
using OpenApiSpecGeneration.Console.Commands.Helpers;

namespace OpenApiSpecGeneration.ApiGeneration.Console.Test;

public class GetOpenApiSpecFileSchemaTests
{
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

        var (key, value) = Assert.Single(result.Value?.Components.Schemas);
        Assert.Equal("ToDoItem", key);
        Assert.Equal("object", value.Type);

        var (propertyKey, propertyValue) = Assert.Single(value.Properties);
        Assert.Equal("report", propertyKey);
        Assert.Equal("array", propertyValue.Type);

        var (propertiesPropertyKey, propertiesPropertyValue) = Assert.Single(propertyValue.Items?.Properties);
        Assert.Equal("account_holders", propertiesPropertyKey);
        Assert.Equal("array", propertiesPropertyValue.Type);

        var (propertiesPropertiesPropertyKey, propertiesPropertiesPropertyValue) = Assert.Single(propertiesPropertyValue.Items?.Properties);
        Assert.Equal("name", propertiesPropertiesPropertyKey);
        Assert.Equal("string", propertiesPropertiesPropertyValue.Type);
    }

    [Fact]
    public async Task ArraySchema_Loads()
    {
        // Arrange
        var getOpenApiSpecFile = new GetOpenApiSpecFile();
        var path = $"TestData/Schema_StringArray.json";

        // Act
        var result = await getOpenApiSpecFile.Execute(path);

        // Assert
        Assert.True(result.IsSuccess);

        var (key, value) = Assert.Single(result.Value?.Components.Schemas);
        Assert.Equal("Address", key);
        Assert.Equal("object", value.Type);

        var (propertyKey, propertyValue) = Assert.Single(value.Properties);
        Assert.Equal("lines", propertyKey);
        Assert.Equal("array", propertyValue.Type);

        Assert.Equal("string", propertyValue.Items?.Type);
        Assert.Empty(propertyValue.Items?.Properties);
    }
}
