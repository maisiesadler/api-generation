using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace OpenApiSpecGeneration.Console;

public class GenerateOpenApiSpecSettings : CommandSettings
{
    [CommandOption("-o|--output")]
    [Description("Output format")]
    public string Name { get; set; } = string.Empty;

    // [CommandOption("-o|--output")]
    // [Description("Output format")]
    // [TypeConverter(typeof(OutputFormatConverter))]
    // [DefaultValue(OutputFormat.ResultOnly)]
    // public OutputFormat OutputFormat { get; set; }
}

public class GenerateOpenApiSpec : AsyncCommand<GenerateOpenApiSpecSettings>
{
    public GenerateOpenApiSpec()
    {
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] GenerateOpenApiSpecSettings settings)
    {
        try
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
                return 1;
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
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }

        return 0;
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
