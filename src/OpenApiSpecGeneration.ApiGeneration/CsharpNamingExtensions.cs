using System.Text.RegularExpressions;

namespace OpenApiSpecGeneration
{
    internal class CsharpNamingExtensions
    {
        internal static string PathToClassName(string apiPath)
        {
            var split = apiPath.Split("/", StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", split.Select(RemoveParameters).Select(FirstLetterToUpper));
        }

        internal static string PathToInteractorType(string apiPath, string method)
        {
            return $"I{FirstLetterToUpper(method)}{PathToClassName(apiPath)}Interactor";
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
    }
}
