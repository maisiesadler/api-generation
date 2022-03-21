using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using OpenApiSpecGeneration.Console.Commands.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace OpenApiSpecGeneration.Console.Commands;

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

internal class GenerateOpenApiSpec : AsyncCommand<GenerateOpenApiSpecSettings>
{
    private readonly GetOpenApiSpecFile _getOpenApiSpecFile;
    private readonly WriteToFile _writeToFile;

    public GenerateOpenApiSpec(
        GetOpenApiSpecFile getOpenApiSpecFile,
        WriteToFile writeToFile)
    {
        _getOpenApiSpecFile = getOpenApiSpecFile;
        _writeToFile = writeToFile;
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] GenerateOpenApiSpecSettings settings)
    {
        try
        {
            AnsiConsole.MarkupLine("Reading from [yellow]{0}[/] and writing to [yellow]{1}[/] with namespace [yellow]{2}[/]", settings.InputFileName, settings.OutputDirectory, settings.Namespace);

            _writeToFile.Initialize(settings.OutputDirectory, new[] { "models", "interactors" });

            var path = Directory.GetCurrentDirectory();
            var result = await _getOpenApiSpecFile.Execute(Path.Combine(path, settings.InputFileName));
            if (!result.IsSuccess)
            {
                System.Console.WriteLine("Could not read file");
                return 1;
            }

            var openApiSpec = result.Value!;

            foreach (var file in FileGenerator.GenerateControllers(settings.Namespace, openApiSpec))
            {
                await _writeToFile.Execute(settings.OutputDirectory, file);
            }

            foreach (var file in FileGenerator.GenerateModels(settings.Namespace, openApiSpec))
            {
                await _writeToFile.Execute(settings.OutputDirectory, file);
            }

            foreach (var file in FileGenerator.GenerateInteractors(settings.Namespace, openApiSpec))
            {
                await _writeToFile.Execute(settings.OutputDirectory, file);
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
}
