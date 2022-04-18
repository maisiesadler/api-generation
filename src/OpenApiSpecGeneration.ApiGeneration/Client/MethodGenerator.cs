using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.ApiGeneration.Client
{
    internal class MethodGenerator
    {
        internal static BlockSyntax CreateMethodBody(TypeSyntax? typeToGenerate, TypeSyntax returnType)
        {
            return SyntaxFactory.Block();
        }
    }
}
