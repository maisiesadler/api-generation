using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    internal class ApiGenerator
    {
        internal static IList<MemberDeclarationSyntax> Generate(OpenApiSpec spec)
        {
            var members = new List<MemberDeclarationSyntax>();
            foreach (var (name, openApiPath) in spec.paths)
            {
                var normalisedName = CsharpNamingExtensions.PathToClassName(name);
                var @class = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(normalisedName))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var (method, openApiMethod) in openApiPath)
                {
                    var methodBody = SyntaxFactory.ParseStatement("");

                    var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), CsharpNamingExtensions.FirstLetterToUpper(method))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(methodBody));

                    @class = @class.AddMembers(methodDeclaration);
                }
                members.Add(@class);
            }

            return members;
        }
    }
}
