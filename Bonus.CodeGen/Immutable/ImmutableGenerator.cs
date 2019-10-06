using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Bonus.CodeGen
{
    public class ImmutableGenerator : ICodeGenerator
    {
        public ImmutableGenerator(AttributeData data)
        {
        }

        private static SyntaxTriviaList Pragma(bool enable, int number)
        {
            return TriviaList(Trivia(
                PragmaWarningDirectiveTrivia(
                        Token(enable ? SyntaxKind.RestoreKeyword : SyntaxKind.DisableKeyword),
                        true)
                    .WithErrorCodes(
                        SingletonSeparatedList<ExpressionSyntax>(
                            LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                Literal(number)))
                    )));
        }
        
        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var targetClass = (ClassDeclarationSyntax)context.ProcessingNode;

            var immutableContext = ImmutableGenerationContext.Create(targetClass, context.Compilation, context.SemanticModel);

            var resultClass = targetClass
                .WithAttributeLists(List(new AttributeListSyntax[0]))
                .WithBaseList(null)
                .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithMembers(List(new MemberDeclarationSyntax[]
                {
                    Ctor(immutableContext),
                    Create(immutableContext),
                    With(immutableContext)
                }));

            return Task.FromResult(List<MemberDeclarationSyntax>().Add(resultClass));
        }

        

        private ConstructorDeclarationSyntax Ctor(ImmutableGenerationContext context)
        {
            var attribute = Attribute(QualifiedName(IdentifierName("System"), IdentifierName("Obsolete")))
                .WithArgumentList(
                    AttributeArgumentList(
                        SingletonSeparatedList(
                            AttributeArgument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal("prefer Create() - used for deserialization")
                                )
                            )
                        )
                    )
                );

            var assignments = context.Properties.Select(property => ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(property.Identifier.Text),
                        IdentifierName(property.Identifier.Text.ToCamelCase())
                    )
                )
            );

            return ConstructorDeclaration(context.ClassIdentifier)
                .WithAttributeLists(
                    SingletonList(
                        AttributeList(SingletonSeparatedList(attribute)
                    )))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(SeparatedList(context.Parameters)))
                .WithBody(Block(assignments));
        }


        private MethodDeclarationSyntax Create(ImmutableGenerationContext context)
        {
            return MethodDeclaration(IdentifierName(context.ClassIdentifier), Identifier("Create"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(ParameterList(SeparatedList(context.OptionalParameters)))
                .WithBody(CreateBody(context));
        }

        private BlockSyntax CreateBody(ImmutableGenerationContext context)
        {
            ExpressionSyntax Get(PropertyDeclarationSyntax property)
            {
                if (context.HasDefault)
                {
                    return InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(property.Identifier.Text.ToCamelCase()),
                                IdentifierName("ValueOr")))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("New"),
                                        IdentifierName(property.Identifier))
                                    )
                                )
                            )
                        );
                }

                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(property.Identifier.Text.ToCamelCase()),
                    IdentifierName("Value")
                );
            }

            var arguments = context.Properties.Select(property =>
                Argument(Get(property))
                    .WithNameColon(NameColon(IdentifierName(property.Identifier.Text.ToCamelCase())))
            );

            var createInstance = ObjectCreationExpression(IdentifierName(context.ClassIdentifier))
                .WithArgumentList(ArgumentList(SeparatedList(arguments)));

            return Block(
                ReturnStatement(createInstance)
                    .WithReturnKeyword(Token(Pragma(false, 618), SyntaxKind.ReturnKeyword, TriviaList()))
            ).WithCloseBraceToken(
                Token(Pragma(true, 618), SyntaxKind.CloseBraceToken, TriviaList())
            );
        }


        private static MethodDeclarationSyntax With(ImmutableGenerationContext context)
        {
            return MethodDeclaration(IdentifierName(context.ClassIdentifier), Identifier("With"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(SeparatedList(context.OptionalParameters)))
                .WithBody(WithBody(context));
        }

        private static BlockSyntax WithBody(ImmutableGenerationContext context)
        {
            var arguments = context.Properties.Select(property =>
                Argument(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(property.Identifier.Text.ToCamelCase()),
                                IdentifierName("ValueOr")))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(IdentifierName(property.Identifier.Text))
                                )
                            )
                        )
                ).WithNameColon(NameColon(IdentifierName(property.Identifier.Text.ToCamelCase())))
            );

            var createInstance = ObjectCreationExpression(IdentifierName(context.ClassIdentifier))
                .WithArgumentList(ArgumentList(SeparatedList(arguments)));

            return Block(
                ReturnStatement(createInstance)
                    .WithReturnKeyword(Token(Pragma(false, 618), SyntaxKind.ReturnKeyword, TriviaList()))
            ).WithCloseBraceToken(
                Token(Pragma(true, 618), SyntaxKind.CloseBraceToken, TriviaList())
            );
        }
    }
}