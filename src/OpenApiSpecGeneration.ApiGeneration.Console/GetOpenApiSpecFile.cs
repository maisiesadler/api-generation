using System.Text.Json;

namespace OpenApiSpecGeneration.Console;

public class GetOpenApiSpecFile
{
    public async Task<Result<OpenApiSpec>> Execute(string path)
    {
        if (!File.Exists(path))
            return Result<OpenApiSpec>.Failure();

        await using var fileStream = File.OpenRead(path);

        var openApiSpec = await JsonSerializer.DeserializeAsync<OpenApiSpec>(fileStream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (openApiSpec != null)
            return Result<OpenApiSpec>.Success(openApiSpec);

        return Result<OpenApiSpec>.Failure();
    }
}
