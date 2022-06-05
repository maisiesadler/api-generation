using System.Text.RegularExpressions;
using OpenApiSpecGeneration.Console.Commands.Helpers;
using Spectre.Console;

namespace OpenApiSpecGeneration.Console.Commands;

public class GenerateFromOpenApiSpecSettings
{
    public string InputFileName { get; init; } = string.Empty;

    public string OutputDirectory { get; init; } = string.Empty;

    public string Namespace { get; init; } = string.Empty;

    public bool GenerateModels { get; init; } = false;

    public bool GenerateControllers { get; init; } = false;

    public bool GenerateInteractors { get; init; } = false;

    public bool GenerateImplementations { get; init; } = false;

    public bool GenerateClients { get; init; } = false;

    public bool GenerateReadme { get; init; } = false;
}

internal class GenerateFromOpenApiSpec
{
    private readonly GetOpenApiSpecFile _getOpenApiSpecFile;
    private readonly WriteToFile _writeToFile;

    public GenerateFromOpenApiSpec(
        GetOpenApiSpecFile getOpenApiSpecFile,
        WriteToFile writeToFile)
    {
        _getOpenApiSpecFile = getOpenApiSpecFile;
        _writeToFile = writeToFile;
    }

    public async Task<int> ExecuteAsync(GenerateFromOpenApiSpecSettings settings)
    {
        try
        {
            AnsiConsole.MarkupLine("Reading from [yellow]{0}[/] and writing to [yellow]{1}[/] with namespace [yellow]{2}[/]", settings.InputFileName, settings.OutputDirectory, settings.Namespace);

            var directoriesToCreate = DirectoriesToCreate(settings);
            _writeToFile.Initialize(settings.OutputDirectory, directoriesToCreate);

            var path = Directory.GetCurrentDirectory();
            var result = await _getOpenApiSpecFile.Execute(Path.Combine(path, settings.InputFileName));
            if (!result.IsSuccess)
            {
                System.Console.WriteLine("Could not read file");
                return 1;
            }

            var openApiSpec = result.Value!;

            var summaries = new List<Summary>();

            if (settings.GenerateControllers)
            {
                var controllers = FileGenerator.GenerateControllers(settings.Namespace, openApiSpec).ToArray();
                summaries.Add(new Summary("Controllers", controllers));
            }

            if (settings.GenerateModels)
            {
                var models = FileGenerator.GenerateModels(settings.Namespace, openApiSpec).ToArray();
                summaries.Add(new Summary("Models", models));
            }

            if (settings.GenerateInteractors)
            {
                var interactors = FileGenerator.GenerateInteractors(settings.Namespace, openApiSpec).ToArray();
                summaries.Add(new Summary("Interactors", interactors));
            }

            if (settings.GenerateImplementations)
            {
                var implementations = ApiGeneration.AutoFixture.FileGenerator.GenerateImplementation(settings.Namespace, openApiSpec).ToArray();
                summaries.Add(new Summary("Implementations", implementations));
            }

            if (settings.GenerateClients)
            {
                var clients = FileGenerator.GenerateClients(settings.Namespace, openApiSpec).ToArray();
                summaries.Add(new Summary("Clients", clients));
            }

            foreach (var (_, files) in summaries)
            {
                foreach (var file in files)
                {
                    await _writeToFile.Execute(settings.OutputDirectory, file);
                }
            }

            if (settings.GenerateReadme)
            {
                var readmeLines = CreateReadme(summaries);
                await _writeToFile.Execute(settings.OutputDirectory, "README.md", readmeLines);
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

    private static string[] CreateReadme(List<Summary> summaries)
    {
        var lines = new List<string>();

        lines.Add("# Generated Files");
        lines.Add("");

        foreach (var (name, files) in summaries)
        {
            lines.Add($"## Generated {name}");

            if (!files.Any())
                continue;

            var first = files.First();
            var location = Regex.Replace(first.fileLocation, "/(:?[^/]+)$", "");
            if (location.Length == 0) location = "/";

            lines.Add($"[{location}](.{location})");

            foreach (var file in files)
            {
                var split = first.fileLocation.Split("/");
                var fileName = split.Last();
                lines.Add($"- `{fileName}`");
            }
            lines.Add("");
        }

        lines.Add("");
        return lines.ToArray();
    }

    private record Summary(string name, WritableFile[] files);

    private static string[] DirectoriesToCreate(GenerateFromOpenApiSpecSettings settings)
    {
        var directories = new List<string>();

        if (settings.GenerateClients)
            directories.Add("clients");

        if (settings.GenerateModels)
            directories.Add("models");

        if (settings.GenerateImplementations)
            directories.Add("implementations");

        if (settings.GenerateInteractors)
            directories.Add("interactors");

        return directories.ToArray();
    }
}
