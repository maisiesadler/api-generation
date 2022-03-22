using Xunit;
using System.Threading.Tasks;
using OpenApiSpecGeneration.Console.Commands.Helpers;

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
}
