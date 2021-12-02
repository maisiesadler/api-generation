using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    internal class Program
    {
        private static async Task Main()
        {
            var path = Directory.GetCurrentDirectory();
            await using var fileStream = File.OpenRead(Path.Combine(path, "definition.json"));

            var outputDirectory = "example/generated";
            var @namespace = "Example";
            SetupOutputDirectory(outputDirectory);

            var openApiSpec = await JsonSerializer.DeserializeAsync<OpenApiSpec>(fileStream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (openApiSpec == null)
            {
                System.Console.WriteLine("Could not read file");
                return;
            }

            foreach (var file in FileGenerator.GenerateControllers(@namespace, openApiSpec))
            {
                await WriteToFile(outputDirectory, file);
            }

            foreach (var file in FileGenerator.GenerateModels(@namespace, openApiSpec))
            {
                await WriteToFile(outputDirectory, file);
            }

            // var interactors = ApiGenerator.GenerateInteractors(openApiSpec);

            // foreach (var interactor in interactors)
            // {
            //     var usings = SyntaxFactory.List<UsingDirectiveSyntax>(new[]{
            //         SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName($"{@namespace}.Models")),
            //     });
            //     var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{@namespace}.Interactors"))
            //         .AddMembers(interactor);

            //     await WriteToFile($"{outputDirectory}/interactors/{interactor.Identifier.Value}.cs", usings, ns);
            // }
        }

        private static async Task WriteToFile(
            string outputDirectory,
            WritableFile writableFile)
        {
            await using var streamWriter = new StreamWriter($"{outputDirectory}/{writableFile.fileLocation}");

            if (writableFile.usingDirectiveSyntax != null)
            {
                foreach (var directive in writableFile.usingDirectiveSyntax)
                {
                    directive.NormalizeWhitespace().WriteTo(streamWriter);
                    streamWriter.WriteLine();
                }
                streamWriter.WriteLine();
            }

            writableFile.namespaceDeclarationSyntax.NormalizeWhitespace().WriteTo(streamWriter);
        }

        private static void SetupOutputDirectory(string location)
        {
            if (Directory.Exists(location))
                Directory.Delete(location, true);
            Directory.CreateDirectory(location);
            Directory.CreateDirectory($"{location}/models");
            Directory.CreateDirectory($"{location}/interactors");
        }
    }

    public record WritableFile(
        string fileLocation,
        SyntaxList<UsingDirectiveSyntax>? usingDirectiveSyntax,
        NamespaceDeclarationSyntax namespaceDeclarationSyntax);

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
