using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynCodeGenExample
{
    internal class Program
    {
        private static async Task Main()
        {
            var path = Directory.GetCurrentDirectory();
            await using var fileStream = File.OpenRead(Path.Combine(path, "definition.json"));

            var schema = await JsonSerializer.DeserializeAsync<OpenApiSpec>(fileStream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (schema == null)
            {
                System.Console.WriteLine("Could not read file");
                return;
            }

            var members = new List<MemberDeclarationSyntax>();
            foreach (var (name, openApiPath) in schema.paths)
            {
                var normalisedName = NormaliseName(name);
                var @class = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(normalisedName))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var (method, openApiMethod) in openApiPath)
                {
                    var methodBody = SyntaxFactory.ParseStatement("");

                    var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), FirstLetterToUpper(method))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(methodBody));

                    @class = @class.AddMembers(methodDeclaration);
                }
                members.Add(@class);
            }

            var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("CodeGen")).AddMembers(members.ToArray());

            await using var streamWriter = new StreamWriter(@"../output/generated.cs", false);
            ns.NormalizeWhitespace().WriteTo(streamWriter);
        }

        private static string NormaliseName(string name)
        {
            var split = name.Split("/", StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", split.Select(RemoveParameters).Select(FirstLetterToUpper));
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
    }

    public class OpenApiSpec
    {
        public IReadOnlyDictionary<string, OpenApiPath> paths { get; init; } = new Dictionary<string, OpenApiPath>();
    }

    public class OpenApiPath : Dictionary<string, OpenApiMethod>
    {
    }

    public class OpenApiMethod
    {
        public IReadOnlyCollection<string> tags { get; init; } = Array.Empty<string>();
    }
}