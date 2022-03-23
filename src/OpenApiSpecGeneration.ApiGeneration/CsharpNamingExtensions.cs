using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration
{
    internal static class CsharpNamingExtensions
    {
        internal static string PathToClassName(string apiPath)
        {
            var split = apiPath.Split("/", StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", split.Select(RemoveParameters).Select(FirstLetterToUpper));
        }

        internal static string PathToInteractorType(string apiPath, OperationType operationType)
        {
            return $"I{FirstLetterToUpper(operationType.ToString())}{PathToClassName(apiPath)}Interactor";
        }

        internal static string InterfaceToPropertyName(string interfaceTypeName)
        {
            if (interfaceTypeName.Length > 2 && interfaceTypeName[0] == 'I')
                return char.ToLower(interfaceTypeName[1]) + interfaceTypeName.Substring(2);

            if (interfaceTypeName.Length > 1)
                return char.ToLower(interfaceTypeName[0]) + interfaceTypeName.Substring(1);

            return interfaceTypeName.ToLower();
        }

        internal static string RemoveParameters(string str)
        {
            return Regex.Replace(str, "[{}]", "");
        }

        internal static string FirstLetterToUpper(string str)
        {
            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        private static string FirstLetterToLower(string str)
        {
            if (str.Length > 1)
                return char.ToLower(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        internal static string SnakeCaseToCamel(string str)
            => SplitToCamel(str, '_');

        internal static string HeaderToParameter(string? str)
        {
            if (str == null) return string.Empty;

            var x = SplitToCamel(str, '_');
            x = SplitToCamel(x, '-');
            return FirstLetterToLower(x);
        }

        private static string SplitToCamel(string str, char splitChar)
        {
            var split = str.Split(splitChar);
            return string.Join("", split.Select(FirstLetterToUpper));
        }
    }
}
