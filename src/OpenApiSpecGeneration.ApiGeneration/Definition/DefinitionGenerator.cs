using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Generatable;

namespace OpenApiSpecGeneration.Definition
{
    public record Definition(Route[] routes, SchemaDefinition[] schemas);
    public record Route(string pathName, Operation[] operations);
    public record Operation(string name, OperationType type, ArgumentDefinition[] arguments, ReturnType returnType);
    public record ReturnType(bool hasReturnType, bool isArray, string value);

    public static class DefinitionExtensions
    {
        public static string NormalisedName(this Route route) => CsharpNamingExtensions.PathEtcToClassName(route.pathName);

        public static string InteractorClass(this Operation operation) => $"{operation.name}Interactor";

        public static string InteractorInterface(this Operation operation) => $"I{operation.name}Interactor";

        public static string InteractorPropertyName(this Operation operation) => $"{char.ToLower(operation.name[0])}{operation.name.Substring(1)}Interactor";
    }

    public class DefinitionGenerator
    {
        public static Definition GenerateDefinition(OpenApiDocument document)
        {
            var routes = GenerateRoutes(document.Paths).ToArray();
            var schemas = SchemaDefinitionGenerator.Execute(document).ToArray();
            return new Definition(routes, schemas);
        }

        private static IEnumerable<Route> GenerateRoutes(OpenApiPaths paths)
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
