using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    internal class ControllerGenerator
    {
        internal static IList<ClassDeclarationSyntax> GenerateControllers(OpenApiSpec spec)
        {
            var members = new List<ClassDeclarationSyntax>();
            foreach (var (apiPath, openApiPath) in spec.paths)
            {
                var normalisedName = CsharpNamingExtensions.PathToClassName(apiPath);

                var classMethods = new List<MethodDeclarationSyntax>();
                var parameters = new List<ParameterSyntax>();
                var fields = new List<MemberDeclarationSyntax>();

                foreach (var (method, openApiMethod) in openApiPath)
                {
                    var propertyType = CsharpNamingExtensions.PathToInteractorType(apiPath, method);
                    var propertyName = CsharpNamingExtensions.InterfaceToPropertyName(propertyType);

                    var methodBody = SyntaxFactory.ParseStatement("");
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), CsharpNamingExtensions.FirstLetterToUpper(method))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(methodBody))
                        .AddAttributeLists(GetMethodAttributeList(method));

                    classMethods.Add(methodDeclaration);
                    fields.Add(CreateField(propertyType, propertyName));
                    parameters.Add(CreateConstructorParameter(propertyType, propertyName));
                }

                var ctor = CreateConstructor(normalisedName, parameters);

                var @class = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(normalisedName))
                    .AddMembers(fields.ToArray())
                    .AddMembers(ctor)
                    .AddMembers(classMethods.ToArray())
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAttributeLists(GetControllerAttributeList(apiPath));

                members.Add(@class);
            }

            return members;
        }

        private static MemberDeclarationSyntax CreateField(string propertyType, string propertyName)
        {
            var fieldTokens = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
            );

            var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(propertyType))
                .AddVariables(SyntaxFactory.VariableDeclarator($"_{propertyName}"));

            var field = SyntaxFactory.FieldDeclaration(
                default,
                fieldTokens,
                variableDeclaration,
                SyntaxFactory.Token(SyntaxKind.SemicolonToken)
            );

            return field;
        }

        private static ConstructorDeclarationSyntax CreateConstructor(string normalisedName, List<ParameterSyntax> parameters)
        {
            var ctorBody = SyntaxFactory.ParseStatement("");
            var ctor = SyntaxFactory.ConstructorDeclaration(normalisedName)
                .AddParameterListParameters(parameters.ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(ctorBody));

            return ctor;
        }

        private static ParameterSyntax CreateConstructorParameter(string propertyType, string propertyName)
        {
            return SyntaxFactory.Parameter(
                default,
                default,
                SyntaxFactory.ParseTypeName(propertyType),
                SyntaxFactory.Identifier(propertyName),
                default
            );
        }

        private static AttributeListSyntax[] GetControllerAttributeList(string route)
        {
            var quoteRoute = $"\"{route}\"";
            var routeArgument = SyntaxFactory.AttributeArgument(
               SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                   SyntaxFactory.Token(SyntaxFactory.TriviaList(),
                       SyntaxKind.StringLiteralToken,
                       quoteRoute,
                       quoteRoute,
                       SyntaxFactory.TriviaList()
                   )
               )
            );
            var routeArgumentList = SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SingletonSeparatedList<AttributeArgumentSyntax>(routeArgument)
            );

            return new[] {
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("ApiController"))
                    )
                ),
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Route"), routeArgumentList)
                    )
                )
            };
        }

        private static AttributeListSyntax[] GetMethodAttributeList(string method)
        {
            var attributeNmae = RouteAttributeName(method);
            return new[] {
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeNmae))
                    )
                ),
            };
        }

        private static string RouteAttributeName(string method)
        {
            return method switch
            {
                "get" => "HttpGet",
                "post" => "HttpPost",
                "put" => "HttpPut",
                "delete" => "HttpDelete",
                _ => throw new InvalidOperationException($"Unknown method '{method}'"),
            };
        }
    }
}
