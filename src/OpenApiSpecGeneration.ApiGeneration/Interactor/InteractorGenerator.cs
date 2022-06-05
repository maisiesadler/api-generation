using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Definition;
using OpenApiSpecGeneration.Generatable;

namespace OpenApiSpecGeneration.Interactor
{
    internal class InteractorGenerator
    {
        internal static IEnumerable<InterfaceDeclarationSyntax> GenerateInteractors(OpenApiDocument document)
        {
            var definition = DefinitionGenerator.GenerateDefinition(document);
            return GenerateInteractors(definition);
        }

        private static IEnumerable<InterfaceDeclarationSyntax> GenerateInteractors(Definition.Definition definition)
        {
            foreach (var route in definition.routes)
            {
                foreach (var operation in route.operations)
                {
                    var interfaceName = CsharpNamingExtensions.PathToInteractorType(route.pathName, operation.type);

                    var parameters = CreateParameterList(operation.arguments);
                    var returnType = ReturnTypeExtensions_2.GetReturnTypeSyntaxAsTask(operation.returnType);
                    var methodDeclaration = SyntaxFactory.MethodDeclaration(
                            returnType,
                            SyntaxFactory.Identifier("Execute"))
                        .WithParameterList(parameters)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    yield return SyntaxFactory.InterfaceDeclaration(interfaceName)
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(methodDeclaration));
                }
            }
        }

        private static ParameterListSyntax CreateParameterList(IList<ArgumentDefinition> arguments)
        {
            if (arguments == null) return SyntaxFactory.ParameterList();

            var parameters = new List<ParameterSyntax>();

            foreach (var argument in arguments)
            {
                var attributeList = SyntaxFactory.List<AttributeListSyntax>();
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(argument.name))
                    .WithType(argument.parameterTypeSyntax);

                parameters.Add(parameter);
            }

            return SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList<ParameterSyntax>(parameters)
            );
        }
    }
}
