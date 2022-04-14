using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace OpenApiSpecGeneration.Generatable
{
    internal class ArgumentDefinitionGenerator
    {
        internal static IEnumerable<ArgumentDefinition> Create(
           string pathName,
           OperationType operationType,
           OpenApiRequestBody? requestBody,
           IList<OpenApiParameter>? openApiMethodParameters)
        {
            var firstContentType = requestBody?.Content.FirstOrDefault();
            if (requestBody != null && firstContentType.HasValue)
            {
                var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromBody"));
                var name = "request";
                var typeName = CsharpNamingExtensions.PathEtcToClassName(
                    new[] { pathName, operationType.ToString(), firstContentType.Value.Key, "Request" });
                yield return new ArgumentDefinition(attribute, SyntaxFactory.ParseTypeName(typeName), name);
            }

            if (openApiMethodParameters != null)
            {
                foreach (var openApiMethodParameter in openApiMethodParameters)
                {
                    var attribute = ParamAttribute(openApiMethodParameter.In, openApiMethodParameter.Name);
                    var name = CsharpNamingExtensions.HeaderToParameter(openApiMethodParameter.Name);
                    var typeSyntax = CsharpTypeExtensions.ParseTypeSyntax(openApiMethodParameter.Schema?.Type);
                    yield return new ArgumentDefinition(attribute, typeSyntax, name);
                }
            }
        }
        private static AttributeSyntax ParamAttribute(ParameterLocation? parameterLocation, string? parameterName)
        {
            AttributeSyntax AsAttribute(string attributeName, string? name = null)
            {
                if (name == null)
                    return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));

                return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName))
                    .WithArgumentList(
                        SyntaxFactory.AttributeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<AttributeArgumentSyntax>(
                                SyntaxFactory
                                    .AttributeArgument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(parameterName ?? string.Empty)))
                                    .WithNameEquals(
                                        SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(name))
                                    )
                            )
                        )
                    );
            }

            return parameterLocation switch
            {
                ParameterLocation.Path => AsAttribute("FromRoute"),
                ParameterLocation.Query => AsAttribute("FromQuery"),
                ParameterLocation.Header => AsAttribute("FromHeader", "Name"),
                _ => throw new InvalidOperationException($"Unknown parameter type '{parameterLocation}'"),
            };
        }
    }
}
