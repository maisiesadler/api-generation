using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.ApiGeneration.Redoc.Console
{
    internal class Program
    {
        private static async Task Main()
        {
            var path = Directory.GetCurrentDirectory();
            await using var fileStream = File.OpenRead(Path.Combine(path, "dist.yaml"));

            var outputDirectory = "redoc-example/generated";
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

            foreach (var file in FileGenerator.GenerateInteractors(@namespace, openApiSpec))
            {
                await WriteToFile(outputDirectory, file);
            }
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
}
