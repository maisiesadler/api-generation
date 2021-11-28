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

            var members = ApiGenerator.GenerateControllers(openApiSpec);

            foreach (var member in members)
            {
                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("CodeGen")).AddMembers(member);

                await using var streamWriter = new StreamWriter($"output/{member.Identifier.Value}.cs", false);
                ns.NormalizeWhitespace().WriteTo(streamWriter);
            }
        }
    }

    public record OpenApiSpec(IReadOnlyDictionary<string, OpenApiPath> paths);

    public class OpenApiPath : Dictionary<string, OpenApiMethod>
    {
    }

    public record OpenApiMethod
    {
        public IReadOnlyCollection<string> tags { get; init; } = Array.Empty<string>();
    }
}
