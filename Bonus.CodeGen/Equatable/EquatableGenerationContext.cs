using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bonus.CodeGen
{
    public class EquatableGenerationContext
    {
        public static EquatableGenerationContext Create(ClassDeclarationSyntax targetClass, CSharpCompilation compilation, SemanticModel semanticModel)
        {
            var properties = targetClass.Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(Filter.PublicNonStatic)
                .ToImmutableArray();

            var nullables = properties
                .Where(property => semanticModel.GetTypeInfo(property.Type).Type.IsReferenceType)
                .ToImmutableHashSet();

            var enumerableType = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
            var stringType = compilation.GetTypeByMetadataName("System.String");

            var enumerables = properties
                .Where(property =>
                {
                    var propertyType = semanticModel.GetTypeInfo(property.Type).Type;
                    return !stringType.Equals(propertyType) &&
                           propertyType.AllInterfaces.Any(@interface => @interface.Equals(enumerableType));
                })
                .ToImmutableHashSet();

            var targetType = semanticModel.GetDeclaredSymbol(targetClass);
            var equatableType = compilation.GetTypeByMetadataName("System.IEquatable`1").Construct(targetType);

            var isIEquatableTDefined = targetType.AllInterfaces.Any(@interface => @interface.Equals(equatableType));

            return new EquatableGenerationContext(targetClass.Identifier, properties, nullables, enumerables, isIEquatableTDefined);
        }

        private readonly ImmutableHashSet<PropertyDeclarationSyntax> _nullable;
        private readonly ImmutableHashSet<PropertyDeclarationSyntax> _enumerables;

        private EquatableGenerationContext(SyntaxToken classIdentifier,
            ImmutableArray<PropertyDeclarationSyntax> properties,
            ImmutableHashSet<PropertyDeclarationSyntax> nullable,
            ImmutableHashSet<PropertyDeclarationSyntax> enumerables, bool isIEquatableTDefined)
        {
            _nullable = nullable;
            _enumerables = enumerables;
            IsIEquatableTDefined = isIEquatableTDefined;
            ClassIdentifier = classIdentifier;
            Properties = properties;
        }

        public SyntaxToken ClassIdentifier { get; }
        public ImmutableArray<PropertyDeclarationSyntax> Properties { get; }
        public bool IsIEquatableTDefined { get; }

        public bool CanBeNull(PropertyDeclarationSyntax property) => _nullable.Contains(property);

        public bool IsEnumerable(PropertyDeclarationSyntax property) => _enumerables.Contains(property);
    }
}