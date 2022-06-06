using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Generatable;

namespace OpenApiSpecGeneration.Definition
{
    internal class OperationGenerator
    {
        internal static Operation Generate(
            string pathName,
            OperationType operationType,
            OpenApiOperation? operation)
        {
            var returnType = ReturnTypeDefintionGenerator.GetReturnType(operation?.Responses);
            var argumentDefinitions = ArgumentDefinitionGenerator.Create(pathName, operationType, operation?.RequestBody, operation?.Parameters).ToArray();

            return new Operation(OperationName(pathName, operationType), operationType, argumentDefinitions, returnType);
        }

        private static string OperationName(string apiPath, OperationType operationType)
            => CsharpNamingExtensions.PathEtcToClassName(new[] { operationType.ToString(), apiPath });
    }
}
