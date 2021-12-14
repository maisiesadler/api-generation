using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenApiSpecGeneration.Entities;

namespace OpenApiSpecGeneration.ImplementationGeneration.Implementation
{
    internal class ImplementationGenerator
    {
        internal static IEnumerable<ClassDeclarationSyntax> GenerateImplementations(OpenApiSpec spec)
        {
            foreach (var (apiPath, openApiPath) in spec.paths)
            {
                foreach (var (method, openApiMethod) in openApiPath.GetMethods())
                {
                    // var returnType = ReturnTypeExtensions.GetReturnTypeSyntax(openApiMethod.responses);
                    // var methodDeclaration = SyntaxFactory.MethodDeclaration(
                    //         returnType,
                    //         SyntaxFactory.Identifier("Execute"))
                    //     .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    // var methods = new[] { methodDeclaration };

                    var implementationName = CsharpNamingExtensions.PathToInteractorImplementationType(apiPath, method);
                    var interfaceName = CsharpNamingExtensions.PathToInteractorInterface(apiPath, method);

                    var @class = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier(implementationName))
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(interfaceName)))
                        // .AddMembers(fields.ToArray())
                        // .AddMembers(ctor)
                        // .AddMembers(classMethods.ToArray())
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                        // .AddAttributeLists(GetControllerAttributeList(apiPath));

                    yield return @class;
                }
            }
        }
    }
}
