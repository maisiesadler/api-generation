using System.Text.RegularExpressions;

namespace OpenApiSpecGeneration
{
    internal class CsharpNamingExtensions
    {
        internal static string PathToClassName(string name)
        {
            var split = name.Split("/", StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", split.Select(RemoveParameters).Select(FirstLetterToUpper));
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
