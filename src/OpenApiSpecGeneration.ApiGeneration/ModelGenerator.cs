using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration
{
    internal class ModelGenerator
    {
        internal static IList<RecordDeclarationSyntax> GenerateModels(OpenApiSpec spec)
        {
            var members = new List<RecordDeclarationSyntax>();
            foreach (var (name, openApiComponentSchema) in spec.components.schemas)
            {
                var parameters = new List<ParameterSyntax>();
                var properties = new List<MemberDeclarationSyntax>();

                foreach (var (propertyName, openApiProperty) in openApiComponentSchema.properties)
                {
                    var property = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), SyntaxFactory.Identifier(propertyName))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                    properties.Add(property);

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
                    parameterList: default,
                    baseList: null,
                    constraintClauses: default,
                    openBraceToken: SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
                    members: SyntaxFactory.List<MemberDeclarationSyntax>(
                        properties.ToArray()
                    ),
                    closeBraceToken: SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
                    semicolonToken: default);

                // var recordDeclaration = SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), SyntaxFactory.Identifier(name))
                //     .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                //     .AddModifiers(SyntaxFactory.Token(SyntaxKind.OpenBracketToken))
                //     .AddMembers(properties.ToArray())
                //     .AddModifiers(SyntaxFactory.Token(SyntaxKind.OpenBracketToken));

                members.Add(recordDeclaration);
            }

            return members;
        }
    }
}
