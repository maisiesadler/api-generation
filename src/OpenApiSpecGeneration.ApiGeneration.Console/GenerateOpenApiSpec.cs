using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace OpenApiSpecGeneration.Console;

public class GenerateOpenApiSpecSettings : CommandSettings
{
    [CommandOption("-i|--input")]
    [Description("Input file name")]
    public string InputFileName { get; init; } = string.Empty;

    [CommandOption("-o|--output")]
    [Description("Output directory")]
    public string OutputDirectory { get; init; } = string.Empty;

    [CommandOption("-n|--namespace")]
    [Description("Namespace of generated files")]
    public string Namespace { get; init; } = string.Empty;

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(InputFileName)) return ValidationResult.Error("InputFileName missing");
        if (string.IsNullOrWhiteSpace(OutputDirectory)) return ValidationResult.Error("OutputDirectory missing");
        if (string.IsNullOrWhiteSpace(Namespace)) return ValidationResult.Error("Namespace missing");

        return ValidationResult.Success();
    }
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
            AnsiConsole.MarkupLine("Reading from [yellow]{0}[/] and writing to [yellow]{1}[/] with namespace [yellow]{2}[/]", settings.InputFileName, settings.OutputDirectory, settings.Namespace);

            var path = Directory.GetCurrentDirectory();
            await using var fileStream = File.OpenRead(Path.Combine(path, settings.InputFileName));

            SetupOutputDirectory(settings.OutputDirectory);

            var openApiSpec = await JsonSerializer.DeserializeAsync<OpenApiSpec>(fileStream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (openApiSpec == null)
            {
                System.Console.WriteLine("Could not read file");
                return 1;
            }

            foreach (var file in FileGenerator.GenerateControllers(settings.Namespace, openApiSpec))
            {
                await WriteToFile(settings.OutputDirectory, file);
            }

            foreach (var file in FileGenerator.GenerateModels(settings.Namespace, openApiSpec))
            {
                await WriteToFile(settings.OutputDirectory, file);
            }

            foreach (var file in FileGenerator.GenerateInteractors(settings.Namespace, openApiSpec))
            {
                await WriteToFile(settings.OutputDirectory, file);
            }

            AnsiConsole.MarkupLine("[green]Done :magic_wand:[/]");
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
