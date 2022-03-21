using Microsoft.CodeAnalysis;

namespace OpenApiSpecGeneration.Console.Commands.Helpers;

internal class WriteToFile
{
    public void Initialize(string outputDirectory, string[] folders)
    {
        if (Directory.Exists(outputDirectory))
            Directory.Delete(outputDirectory, true);
        Directory.CreateDirectory(outputDirectory);

        foreach (var folder in folders)
            Directory.CreateDirectory($"{outputDirectory}/{folder}");
    }

    public async Task Execute(
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
}
