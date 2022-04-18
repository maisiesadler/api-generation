using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenApiSpecGeneration.Generatable;

namespace OpenApiSpecGeneration.Interactor
{
    internal class InteractorGenerator
    {
        internal static IEnumerable<InterfaceDeclarationSyntax> GenerateInteractors(OpenApiDocument document)
        {
            foreach (var (apiPath, openApiPath) in document.Paths)
            {
                foreach (var (operationType, operation) in openApiPath.Operations)
                {
                    var interfaceName = CsharpNamingExtensions.PathToInteractorType(apiPath, operationType);

                    var argumentDefinitions = ArgumentDefinitionGenerator.Create(apiPath, operationType, operation.RequestBody, operation.Parameters).ToArray();
                    var parameters = CreateParameterList(argumentDefinitions);
                    var returnType = ReturnTypeExtensions.GetReturnTypeSyntaxAsTask(operation.Responses);
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

        private static ParameterListSyntax CreateParameterList(IList<ArgumentDefinition> openApiMethodParameters)
        {
            if (openApiMethodParameters == null) return SyntaxFactory.ParameterList();

            var parameters = new List<ParameterSyntax>();

            foreach (var openApiMethodParameter in openApiMethodParameters)
            {
                var attributeList = SyntaxFactory.List<AttributeListSyntax>();
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(openApiMethodParameter.name))
                    .WithType(openApiMethodParameter.parameterTypeSyntax);

                parameters.Add(parameter);
            }

            return SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList<ParameterSyntax>(parameters)
            );
        }
    }
}
