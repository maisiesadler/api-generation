// using System.Collections.Generic;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using Xunit;

// namespace OpenApiSpecGeneration.ApiGeneration.Test;

// public class GenerateInteractorsFileTests
// {
//     [Fact]
//     public void InteractorFileNamespacesAndFileNameCorrect()
//     {
//         // Arrange
//         var openApiContentSchema = new OpenApiContentSchema("array", new Dictionary<string, string>());
//         var openApiResponse = new OpenApiResponse("Success", new Dictionary<string, OpenApiContent>
//         {
//             { "text/plain", new OpenApiContent(openApiContentSchema) }
//         });
//         var openApiResponses = new Dictionary<string, OpenApiResponse> { { "200", openApiResponse } };
//         var apiTestPath = new OpenApiPath
//         {
//             get = new OpenApiMethod { responses = openApiResponses },
//         };
//         var paths = new Dictionary<string, OpenApiPath>
//         {
//             { "/api/test", apiTestPath },
//         };
//         var spec = new OpenApiSpec(paths, new OpenApiComponent(new Dictionary<string, OpenApiComponentSchema>()));

//         // Act
//         var writableFiles = FileGenerator.GenerateInteractors("MyNamespace", spec);

//         // Assert
//         var writableFile = Assert.Single(writableFiles);
//         Assert.Equal("/interactors/IGetApiTestInteractor.cs", writableFile.fileLocation);
//         Assert.Equal("namespace", writableFile.namespaceDeclarationSyntax.NamespaceKeyword.Value);
//         var namespaceIdentifier = Assert.IsType<QualifiedNameSyntax>(writableFile.namespaceDeclarationSyntax.Name);
//         var leftNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Left);
//         Assert.Equal("MyNamespace", leftNamespaceIdentifier.Identifier.Value);
//         var rightNamespaceIdentifier = Assert.IsType<IdentifierNameSyntax>(namespaceIdentifier.Right);
//         Assert.Equal("Interactors", rightNamespaceIdentifier.Identifier.Value);
//         var usingDirective = Assert.Single(writableFile.usingDirectiveSyntax!.Value);
//         var usingName = Assert.IsType<IdentifierNameSyntax>(usingDirective.Name);
//         Assert.Equal("MyNamespace.Models", usingName.Identifier.Value);
//     }
// }
