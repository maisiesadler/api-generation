using System.Text.Json;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace OpenApiSpecGeneration.Console.Commands.Helpers;

internal class GetOpenApiSpecFile
{
    public async Task<Result<OpenApiDocument>> Execute(string path)
    {
        if (!File.Exists(path))
            return Result<OpenApiDocument>.Failure();

        await using var fileStream = File.OpenRead(path);

        var openApiDocument = new OpenApiStreamReader().Read(fileStream, out var diagnostic);

        if (openApiDocument != null)
            return Result<OpenApiDocument>.Success(openApiDocument);

        return Result<OpenApiDocument>.Failure();
    }
}
