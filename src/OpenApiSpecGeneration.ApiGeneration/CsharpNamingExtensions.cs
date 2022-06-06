using System.Text;
using System.Text.RegularExpressions;

namespace OpenApiSpecGeneration
{
    internal static class CsharpNamingExtensions
    {
        internal static string PathEtcToClassName(params string?[] parts)
        {
            var sofar = new StringBuilder();
            foreach (var part in parts)
            {
                if (part == null) continue;
                var split = part.Split("/", StringSplitOptions.RemoveEmptyEntries);
                sofar.Append(string.Join("", split.Select(RemoveParameters).Select(FirstLetterToUpper)));
            }

            return sofar.ToString();
        }

        private static string RemoveParameters(string str)
        {
            return Regex.Replace(str, "[{}]", "");
        }

        private static string FirstLetterToUpper(string str)
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
