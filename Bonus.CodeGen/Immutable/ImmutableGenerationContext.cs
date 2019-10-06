using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Bonus.CodeGen
{
    internal class ImmutableGenerationContext
    {
        public static ImmutableGenerationContext Create(ClassDeclarationSyntax targetClass,
            CSharpCompilation compilation, SemanticModel semanticModel)
        {
            var properties = targetClass.Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(Filter.PublicReadOnly)
                .ToImmutableArray();

            var parameters = properties
                .Select(property => Parameter(property.ToCamelCase()).WithType(property.Type))
                .ToImmutableArray();
            
            var optionalParameters = properties
                .Select(property => OptionalType(property))
                .ToImmutableArray();

            var hasDefault = targetClass.Members
                .OfType<FieldDeclarationSyntax>()
                .Any(Filter.PublicStaticReadOnlyNew);

            return new ImmutableGenerationContext(
                targetClass.Identifier,
                properties,
                parameters,
                optionalParameters,
                hasDefault
            );
        }

        private static ParameterSyntax OptionalType(PropertyDeclarationSyntax property)
        {
            return Parameter(property.ToCamelCase())
                .WithType(
                    QualifiedName(
                        QualifiedName(IdentifierName("Bonus"), IdentifierName("CodeGen")),
                        GenericName(Identifier("Optional"))
                            .WithTypeArgumentList(TypeArgumentList(
                                SingletonSeparatedList(property.Type)))
                    )
                )
                .WithDefault(
                    EqualsValueClause(
                        LiteralExpression(
                            SyntaxKind.DefaultLiteralExpression,
                            Token(SyntaxKind.DefaultKeyword)
                        )
                    )
                );
        }


        private ImmutableGenerationContext(in SyntaxToken classIdentifier,
            IEnumerable<PropertyDeclarationSyntax> properties, IEnumerable<ParameterSyntax> parameters,
            IEnumerable<ParameterSyntax> optionalParameters, bool hasDefault)
        {
            ClassIdentifier = classIdentifier;
            Properties = properties;
            Parameters = parameters;
            OptionalParameters = optionalParameters;
            HasDefault = hasDefault;
        }

        public SyntaxToken ClassIdentifier { get; }
        public IEnumerable<PropertyDeclarationSyntax> Properties { get; }
        public IEnumerable<ParameterSyntax> Parameters { get; }
        public IEnumerable<ParameterSyntax> OptionalParameters { get; }
        public bool HasDefault { get; }
    }
}