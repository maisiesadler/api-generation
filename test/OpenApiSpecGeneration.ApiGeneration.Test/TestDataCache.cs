using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OpenApiSpecGeneration.ApiGeneration.Test;

public static class TestDataCache
{
    private static Dictionary<string, string> _loaded = new();

    public static string Get(string fileName)
    {
        if (_loaded.TryGetValue(fileName, out var contents)) return contents;

        var fileContents = File.ReadAllText($"TestData/{fileName}.json");
        _loaded[fileName] = fileContents;

        return fileContents;
    }

    public static T Get<T>(string fileName)
    {
       var contents = Get(fileName);
       return JsonSerializer.Deserialize<T>(contents)!; 
    }
}
