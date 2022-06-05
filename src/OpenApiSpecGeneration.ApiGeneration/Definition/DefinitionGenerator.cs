using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Generatable;

namespace OpenApiSpecGeneration.Definition
{
    internal record Definition(Route[] routes, SchemaDefinition[] schemas);
    internal record Route(string pathName, Operation[] operations);
    internal record Operation(OperationType type, ArgumentDefinition[] arguments, ReturnType returnType);
    internal record ReturnType(bool hasReturnType, bool isArray, string value);

    internal static class DefinitionExtensions
    {
        internal static string NormalisedName(this Route route) => CsharpNamingExtensions.PathToClassName(route.pathName);
    }

    internal class DefinitionGenerator
    {
        internal static Definition GenerateDefinition(OpenApiDocument document)
        {
            var routes = GenerateRoutes(document.Paths).ToArray();
            var schemas = SchemaDefinitionGenerator.Execute(document).ToArray();
            return new Definition(routes, schemas);
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
