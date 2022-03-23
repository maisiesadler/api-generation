using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public static class TestDataCache
{
    private static Dictionary<string, OpenApiDocument> _loaded = new();

    public static async Task<OpenApiDocument> Get(string fileName)
    {
        if (_loaded.TryGetValue(fileName, out var cached)) return cached;

        await using var fileStream = File.OpenRead($"TestData/{fileName}.json");
        var openApiDocument = new OpenApiStreamReader().Read(fileStream, out var diagnostic);

        _loaded[fileName] = openApiDocument;

        return openApiDocument;
    }
}
