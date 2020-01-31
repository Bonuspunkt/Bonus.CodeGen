using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bonus.CodeGen
{
    internal static class Filter
    {
        public static bool PublicStaticReadOnlyNew(this FieldDeclarationSyntax field) =>
            field.Modifiers.Any(SyntaxKind.PublicKeyword) &&
            field.Modifiers.Any(SyntaxKind.StaticKeyword) &&
            field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) &&
            field.Declaration.Variables.Any(variable => variable.Identifier.Text == "New");

        public static bool PublicNonStatic(this PropertyDeclarationSyntax property) =>
            property.Modifiers.Any(SyntaxKind.PublicKeyword) &&
            !property.Modifiers.Any(SyntaxKind.StaticKeyword);

        public static bool PublicReadOnly(this PropertyDeclarationSyntax property) =>
            property.PublicNonStatic() &&
            property.HasBodyLessGetter() &&
            property.HasNoSetter();

        private static bool HasBodyLessGetter(this PropertyDeclarationSyntax property) =>
            property.AccessorList != null &&
            property.AccessorList.Accessors.Any(accessor =>
                accessor.Kind() == SyntaxKind.GetAccessorDeclaration && accessor.Body == null);

        private static bool HasNoSetter(this PropertyDeclarationSyntax property) =>
            property.AccessorList != null &&
            !property.AccessorList.Accessors.Any(SyntaxKind.SetAccessorDeclaration);

    }
}
