using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.SdkGeneration.Test;

public static class TestExtensions
{
    internal static IEnumerable<T> GetMembersOfType<T>(this SyntaxList<MemberDeclarationSyntax> members)
    {
        foreach (var member in members)
        {
            if (member is T t)
                yield return t;
        }
    }
}
