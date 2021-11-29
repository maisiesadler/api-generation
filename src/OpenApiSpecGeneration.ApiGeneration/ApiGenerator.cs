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

        public static IList<RecordDeclarationSyntax> GenerateModels(OpenApiSpec spec)
        {
            var members = new List<RecordDeclarationSyntax>();
            foreach (var (name, openApiComponentSchema) in spec.components.schemas)
            {
                var parameters = new List<ParameterSyntax>();

                foreach (var (propertyName, openApiProperty) in openApiComponentSchema.properties)
                {
                    // var parameter = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), SyntaxFactory.Identifier(propertyName))
                    //     .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    //     .AddAccessorListAccessors(
                    //         SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    //         SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                    var parameter = SyntaxFactory.Parameter(
                        default,
                        default,
                        SyntaxFactory.ParseTypeName("int"), // TypeSyntax? type,
                        SyntaxFactory.Identifier(propertyName),
                        default
                    );

                    parameters.Add(parameter);
                }

                var tokens = parameters.Skip(1).Select(_ => SyntaxFactory.Token(SyntaxKind.CommaToken));

                var recordDeclaration = SyntaxFactory.RecordDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                    keyword: SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                    identifier: SyntaxFactory.Identifier(name),
                    typeParameterList: default,
                    parameterList: SyntaxFactory.ParameterList(
                        SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                        SyntaxFactory.SeparatedList<ParameterSyntax>(parameters, tokens),
                        SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                    ),
                    baseList: null,
                    constraintClauses: default,
                    openBraceToken: default,
                    // members: SyntaxFactory.SeparatedList<MemberDeclarationSyntax>(

                    // ),
                    members: default,
                    closeBraceToken: default,
                    semicolonToken: SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                members.Add(recordDeclaration);
            }

            return members;
        }
    }
}
