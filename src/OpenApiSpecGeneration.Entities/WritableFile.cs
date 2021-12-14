using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenApiSpecGeneration.Entities;

public record WritableFile(
    string fileLocation,
    SyntaxList<UsingDirectiveSyntax>? usingDirectiveSyntax,
    NamespaceDeclarationSyntax namespaceDeclarationSyntax);
