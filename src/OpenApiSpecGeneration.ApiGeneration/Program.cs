using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace OpenApiSpecGeneration
{
    internal class Program
    {
        private static async Task Main()
        {
            var path = Directory.GetCurrentDirectory();
            await using var fileStream = File.OpenRead(Path.Combine(path, "definition.json"));

            var openApiSpec = await JsonSerializer.DeserializeAsync<OpenApiSpec>(fileStream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (openApiSpec == null)
            {
                System.Console.WriteLine("Could not read file");
                return;
            }

            var members = ApiGenerator.Generate(openApiSpec);
            var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("CodeGen")).AddMembers(members.ToArray());

            await using var streamWriter = new StreamWriter(@"output/generated.cs", false);
            ns.NormalizeWhitespace().WriteTo(streamWriter);
        }
    }

    public class OpenApiSpec
    {
        public IReadOnlyDictionary<string, OpenApiPath> paths { get; init; } = new Dictionary<string, OpenApiPath>();
    }

    public class OpenApiPath : Dictionary<string, OpenApiMethod>
    {
    }

    public class OpenApiMethod
    {
        public IReadOnlyCollection<string> tags { get; init; } = Array.Empty<string>();
    }
}
