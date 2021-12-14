using System.Text.RegularExpressions;

namespace OpenApiSpecGeneration.Entities
{
    public class CsharpNamingExtensions
    {
        public static string PathToClassName(string apiPath)
        {
            var split = apiPath.Split("/", StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", split.Select(RemoveParameters).Select(FirstLetterToUpper));
        }

        public static string PathToInteractorInterface(string apiPath, string method)
        {
            return $"I{PathToInteractorImplementationType(apiPath, method)}";
        }

        public static string PathToInteractorImplementationType(string apiPath, string method)
        {
            return $"{FirstLetterToUpper(method)}{PathToClassName(apiPath)}Interactor";
        }

        public static string InterfaceToPropertyName(string interfaceTypeName)
        {
            if (interfaceTypeName.Length > 2 && interfaceTypeName[0] == 'I')
                return char.ToLower(interfaceTypeName[1]) + interfaceTypeName.Substring(2);

            if (interfaceTypeName.Length > 1)
                return char.ToLower(interfaceTypeName[0]) + interfaceTypeName.Substring(1);

            return interfaceTypeName.ToLower();
        }

        public static string RemoveParameters(string str)
        {
            return Regex.Replace(str, "[{}]", "");
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static string SnakeCaseToCamel(string str)
        {
            var split = str.Split("_");
            return string.Join("", split.Select(FirstLetterToUpper));
        }
    }
}
