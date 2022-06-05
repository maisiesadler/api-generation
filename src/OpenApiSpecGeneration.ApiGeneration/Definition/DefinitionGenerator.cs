using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Generatable;

namespace OpenApiSpecGeneration.Definition
{
    internal record Definition(Route[] routes);
    internal record Route(string pathName, Operation[] operations);
    internal record Operation(OperationType type, bool hasReturnType, ArgumentDefinition[] arguments);

    internal static class DefinitionExtensions
    {
        internal static string NormalisedName(this Route route) => CsharpNamingExtensions.PathToClassName(route.pathName);
    }

    internal class DefinitionGenerator
    {
        internal static Definition GenerateDefinition(OpenApiDocument document)
        {
            var routes = GenerateRoutes(document.Paths).ToArray();
            return new Definition(routes);
        }

        internal static IEnumerable<Route> GenerateRoutes(OpenApiPaths paths)
        {
            foreach (var (apiPath, openApiPath) in paths)
            {
                var operations = new List<Operation>();
                foreach (var (operationType, openApiOperation) in openApiPath.Operations)
                {
                    var operation = OperationGenerator.Generate(apiPath, operationType, openApiOperation);
                    operations.Add(operation);
                }

                yield return new Route(apiPath, operations.ToArray());
            }
        }
    }
}
