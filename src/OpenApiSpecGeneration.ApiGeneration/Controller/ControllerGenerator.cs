using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Controller
{
    internal class ControllerGenerator
    {
        internal static IEnumerable<ClassDeclarationSyntax> GenerateControllers(OpenApiDocument document)
        {
            var members = new List<ClassDeclarationSyntax>();
            foreach (var (apiPath, openApiPath) in document.Paths)
            {
                var normalisedName = CsharpNamingExtensions.PathToClassName(apiPath);

                var classMethods = new List<MethodDeclarationSyntax>();
                var assignments = new List<StatementSyntax>();
                var parameters = new List<ParameterSyntax>();
                var fields = new List<MemberDeclarationSyntax>();

                foreach (var (operationType, operation) in openApiPath.Operations)
                {
                    var interactorType = CsharpNamingExtensions.PathToInteractorType(apiPath, operationType);
                    var interactorPropertyName = CsharpNamingExtensions.InterfaceToPropertyName(interactorType);

                    classMethods.Add(MethodGenerator.CreateMethod(apiPath, operationType, operation, interactorType, interactorPropertyName));
                    fields.Add(CreateField(interactorType, interactorPropertyName));
                    assignments.Add(CreateAssignment(interactorPropertyName));
                    parameters.Add(CreateConstructorParameter(interactorType, interactorPropertyName));
                }

                var ctor = CreateConstructor(normalisedName, parameters, assignments);

                var @class = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(normalisedName))
                    .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName("ControllerBase")))
                    .AddMembers(fields.ToArray())
                    .AddMembers(ctor)
                    .AddMembers(classMethods.ToArray())
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddAttributeLists(GetControllerAttributeList(apiPath));

                yield return @class;
            }
        }

        private static MemberDeclarationSyntax CreateField(string interactorType, string interactorPropertyName)
        {
            var fieldTokens = SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
            );

            var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(interactorType))
                .AddVariables(SyntaxFactory.VariableDeclarator($"_{interactorPropertyName}"));

            return SyntaxFactory.FieldDeclaration(variableDeclaration)
                .WithModifiers(fieldTokens);
        }

        private static ConstructorDeclarationSyntax CreateConstructor(
            string normalisedName,
            List<ParameterSyntax> parameters,
            List<StatementSyntax> assignments)
        {
            var ctor = SyntaxFactory.ConstructorDeclaration(normalisedName)
                .AddParameterListParameters(parameters.ToArray())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(assignments));

            return ctor;
        }

        private static StatementSyntax CreateAssignment(string interactorPropertyName)
        {
            var fieldIdentifier = SyntaxFactory.IdentifierName($"_{interactorPropertyName}");
            var propertyIdentifier = SyntaxFactory.IdentifierName(interactorPropertyName);
            var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, propertyIdentifier);
            return SyntaxFactory.ExpressionStatement(assignment);
        }

        private static ParameterSyntax CreateConstructorParameter(string interactorType, string interactorPropertyName)
        {
            return SyntaxFactory.Parameter(SyntaxFactory.Identifier(interactorPropertyName))
                .WithType(SyntaxFactory.ParseTypeName(interactorType));
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
    }
}
