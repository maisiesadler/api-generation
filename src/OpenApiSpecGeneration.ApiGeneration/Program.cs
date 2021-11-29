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

            var models = ApiGenerator.GenerateModels(openApiSpec);

            foreach (var model in models)
            {
                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("CodeGen.Models")).AddMembers(model);

                await using var streamWriter = new StreamWriter($"output/models/{model.Identifier.Value}.cs", false);
                ns.NormalizeWhitespace().WriteTo(streamWriter);
            }

            var interactors = ApiGenerator.GenerateInteractors(openApiSpec);

            foreach (var interactor in interactors)
            {
                var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("CodeGen.Interactors")).AddMembers(interactor);

                await using var streamWriter = new StreamWriter($"output/interactors/{interactor.Identifier.Value}.cs", false);
                ns.NormalizeWhitespace().WriteTo(streamWriter);
            }
        }
    }

    public record OpenApiSpec(IReadOnlyDictionary<string, OpenApiPath> paths, OpenApiComponent components);

    public class OpenApiPath : Dictionary<string, OpenApiMethod>
    {
    }

    public record OpenApiMethod
    {
        public IReadOnlyCollection<string> tags { get; init; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, OpenApiResponse> responses { get; init; } = new Dictionary<string, OpenApiResponse>();
    }

    public record OpenApiResponse(string description, IReadOnlyDictionary<string, OpenApiContent> content);

    public record OpenApiContent(OpenApiContentSchema schema);

    public record OpenApiContentSchema(string type, IReadOnlyDictionary<string, string> items);

    public record OpenApiComponent(IReadOnlyDictionary<string, OpenApiComponentSchema> schemas);
    public record OpenApiComponentSchema(string type, IReadOnlyDictionary<string, OpenApiComponentProperty> properties);
    public record OpenApiComponentProperty(string type, string? format, bool? nullable);
}
