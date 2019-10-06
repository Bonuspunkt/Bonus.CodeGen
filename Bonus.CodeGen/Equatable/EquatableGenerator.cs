using System;
using System.Collections.Generic;
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
    public class EquatableGenerator : ICodeGenerator
    {
        public EquatableGenerator(AttributeData data)
        {
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var targetClass = (ClassDeclarationSyntax)context.ProcessingNode;

            var equatableContext = EquatableGenerationContext.Create(targetClass, context.Compilation, context.SemanticModel);

            var resultClass = targetClass
                .WithBaseList(GetBaseList(equatableContext))
                .WithAttributeLists(List(new AttributeListSyntax[0]))
                .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithMembers(List(new MemberDeclarationSyntax[]
                {
                    EqualsImplementation(equatableContext),
                    EquatableImplementation(equatableContext),
                    GetHashCode(equatableContext)
                }));

            return Task.FromResult(List<MemberDeclarationSyntax>().Add(resultClass));
        }

        private BaseListSyntax GetBaseList(EquatableGenerationContext context)
        {
            if (context.IsIEquatableTDefined)
            {
                return null;
            }

            return BaseList(
                SingletonSeparatedList<BaseTypeSyntax>(
                    SimpleBaseType(
                        QualifiedName(IdentifierName("System"), GenericName(Identifier("IEquatable"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(IdentifierName(context.ClassIdentifier))
                                )
                            )
                        )
                    )
                )
            );
        }

        private static MethodDeclarationSyntax EqualsImplementation(EquatableGenerationContext context)
        {
            return MethodDeclaration(
                PredefinedType(Token(SyntaxKind.BoolKeyword)),
                Identifier("Equals")
            ).AddModifiers(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.OverrideKeyword)
            ).AddParameterListParameters(
                Parameter(Identifier("obj"))
                    .WithType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
            ).AddBodyStatements(
                ReturnStatement(
                    InvocationExpression(IdentifierName("Equals"))
                        .AddArgumentListArguments(
                            Argument(
                                BinaryExpression(
                                    SyntaxKind.AsExpression,
                                    IdentifierName("obj"),
                                    IdentifierName(context.ClassIdentifier)
                                )
                            )
                        )
                )
            );
        }

        private static MethodDeclarationSyntax EquatableImplementation(EquatableGenerationContext context)
        {
            var notNullCheck = BinaryExpression(SyntaxKind.NotEqualsExpression,
                IdentifierName("other"),
                LiteralExpression(SyntaxKind.NullLiteralExpression)
            );

            var body = context.Properties.Select(property => EqualsCondition(property, context.IsEnumerable(property)))
                .Aggregate(
                    notNullCheck,
                    (prev, curr) => BinaryExpression(SyntaxKind.LogicalAndExpression, prev, curr)
                );


            return MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.BoolKeyword)),
                    Identifier("Equals")
                )
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("other")).WithType(IdentifierName(context.ClassIdentifier))
                )
                .AddBodyStatements(ReturnStatement(body));
        }

        private static InvocationExpressionSyntax EqualsCondition(PropertyDeclarationSyntax property, bool enumerable)
        {
            if (enumerable)
            {
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("System"),
                                    IdentifierName("Linq")),
                                IdentifierName("Enumerable")),
                            IdentifierName("SequenceEqual")))
                    .AddArgumentListArguments(
                        Argument(IdentifierName(property.Identifier)),
                        Argument(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("other"),
                                IdentifierName(property.Identifier)))
                    );
            }
            return InvocationExpression(
                IdentifierName("Equals")
            ).AddArgumentListArguments(
                Argument(IdentifierName(property.Identifier)),
                Argument(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("other"),
                        IdentifierName(property.Identifier)
                    )
                )
            );
        }

        private static MethodDeclarationSyntax GetHashCode(EquatableGenerationContext context)
        {
            var statements = new List<StatementSyntax>()
            {
                LocalDeclarationStatement(
                    VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                        .AddVariables(
                            VariableDeclarator(Identifier("HashingBase"))
                                .WithInitializer(EqualsValueClause(CastExpression(
                                    PredefinedType(Token(SyntaxKind.IntKeyword)),
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(2166136261))
                                )))
                        )
                ).AddModifiers(Token(SyntaxKind.ConstKeyword)),
                LocalDeclarationStatement(
                    VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                        .AddVariables(
                            VariableDeclarator(Identifier("HashingMultiplier"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(16777619))
                                    )
                                )
                        )
                ).AddModifiers(Token(SyntaxKind.ConstKeyword)),
                LocalDeclarationStatement(
                    VariableDeclaration(IdentifierName("var"))
                        .AddVariables(
                            VariableDeclarator(Identifier("hashCode"))
                                .WithInitializer(EqualsValueClause(IdentifierName("HashingBase")))
                        )
                )
            };

            statements.AddRange(context.Properties.Select(property => HashCodeForProperty(property, context.CanBeNull(property))));
            statements.Add(ReturnStatement(IdentifierName("hashCode")));

            return MethodDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier("GetHashCode"))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddModifiers(Token(SyntaxKind.OverrideKeyword))
                .AddBodyStatements(
                    CheckedStatement(
                        SyntaxKind.UncheckedStatement,
                        Block(statements.ToArray())
                    )
                );
        }

        private static ExpressionStatementSyntax HashCodeForProperty(PropertyDeclarationSyntax property, bool canBeNull)
        {

            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("hashCode"),
                    BinaryExpression(
                        SyntaxKind.ExclusiveOrExpression,
                        ParenthesizedExpression(
                            BinaryExpression(
                                SyntaxKind.MultiplyExpression,
                                IdentifierName("hashCode"),
                                IdentifierName("HashingMultiplier")
                            )
                        ),

                        canBeNull
                        ? (ExpressionSyntax)ParenthesizedExpression(
                            BinaryExpression(
                                SyntaxKind.CoalesceExpression,
                                ConditionalAccessExpression(
                                    IdentifierName(property.Identifier),
                                    InvocationExpression(
                                        MemberBindingExpression(
                                            IdentifierName("GetHashCode")
                                        )
                                    )
                                ),
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(0)
                                )
                            )
                        )
                        : (ExpressionSyntax)InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(property.Identifier),
                                IdentifierName("GetHashCode")
                            )
                        )
                    )
                )
            );
        }
    }
}