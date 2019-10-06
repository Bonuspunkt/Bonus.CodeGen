using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bonus.CodeGen
{
    internal static class Extensions
    {
        public static SyntaxToken ToCamelCase(this PropertyDeclarationSyntax property)
        {
            var text = property.Identifier.Text;
            return SyntaxFactory.Identifier(text.ToCamelCase());
        }

        public static string ToCamelCase(this string text)
        {
            return text.Substring(0, 1).ToLower() + text.Substring(1);
        }
    }
}