namespace OpenApiSpecGeneration
{
    internal static class OpenApiSpecExtensions
    {
        internal static IEnumerable<(string, OpenApiMethod)> GetMethods(this OpenApiPath openApiPath)
        {
            if (openApiPath.get != null) yield return ("get", openApiPath.get);
            if (openApiPath.post != null) yield return ("post", openApiPath.post);
            if (openApiPath.put != null) yield return ("put", openApiPath.put);
            if (openApiPath.delete != null) yield return ("delete", openApiPath.delete);
        }
    }
}
