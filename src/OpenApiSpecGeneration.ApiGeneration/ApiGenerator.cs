using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    public class ApiGenerator
    {
        public static IList<ClassDeclarationSyntax> GenerateControllers(OpenApiSpec spec)
        {
            var members = new List<ClassDeclarationSyntax>();
            foreach (var (name, openApiPath) in spec.paths)
            {
                var normalisedName = CsharpNamingExtensions.PathToClassName(name);

                var ctorBody = SyntaxFactory.ParseStatement("");
                var ctor = SyntaxFactory.ConstructorDeclaration(normalisedName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block(ctorBody));
                var @class = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(normalisedName))
                    .AddMembers(ctor)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

                foreach (var (method, openApiMethod) in openApiPath)
                {
                    var methodBody = SyntaxFactory.ParseStatement("");

                    // var attributeArgument = SyntaxFactory.AttributeArgument(
                    //     SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    //         SyntaxFactory.Token(SyntaxFactory.TriviaList(),
                    //             SyntaxKind.StringLiteralToken,
                    //             "some_param",
                    //             "some_param",
                    //             SyntaxFactory.TriviaList()
                    //         )
                    //     )
                    // );

                    // var attributes = SyntaxFactory.AttributeList(
                    //     SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                    //         SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("CustomAttribute"),
                    //         SyntaxFactory.AttributeArgumentList(
                    //             SyntaxFactory.SingletonSeparatedList<AttributeArgumentSyntax>(attributeArgument)))
                    //     )
                    // );

                    var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), CsharpNamingExtensions.FirstLetterToUpper(method))
                        // .AddAttributeLists(attributes)
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
