using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace OpenApiSpecGeneration.Console.Commands;

public class GenerateOpenApiSpecWithMockImplementationSettings : CommandSettings
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

internal class GenerateOpenApiSpecWithMockImplementation : AsyncCommand<GenerateOpenApiSpecSettings>
{
    private readonly GenerateFromOpenApiSpec _generateFromOpenApiSpec;

    public GenerateOpenApiSpecWithMockImplementation(
        GenerateFromOpenApiSpec generateFromOpenApiSpec)
    {
        _generateFromOpenApiSpec = generateFromOpenApiSpec;
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] GenerateOpenApiSpecSettings settings)
    {
        try
        {
            var generateFromOpenApiSpecSettings = new GenerateFromOpenApiSpecSettings
            {
                GenerateReadme = true,
                GenerateControllers = true,
                GenerateImplementations = true,
                GenerateModels = true,
                GenerateInteractors = true,
                GenerateClients = true,
                InputFileName = settings.InputFileName,
                OutputDirectory = settings.OutputDirectory,
                Namespace = settings.Namespace,
            };

            return await _generateFromOpenApiSpec.ExecuteAsync(generateFromOpenApiSpecSettings);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}
