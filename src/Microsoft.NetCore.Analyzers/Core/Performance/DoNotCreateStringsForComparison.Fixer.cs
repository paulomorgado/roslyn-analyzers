// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace Microsoft.NetCore.Analyzers.Performance

{
    /// <summary>
    /// CA1830: Do not create strings for comparison.
    /// </summary>
    public abstract class DoNotCreateStringsForComparisonFixer : CodeFixProvider
    {
        protected static readonly ImmutableArray<string> All = ImmutableArray.Create(
            "OrdinalIgnoreCase",
            "CurrentCultureIgnoreCase",
            "InvariantCultureIgnoreCase");

        protected static readonly ImmutableArray<string> InvariantCulture = ImmutableArray.Create(
            "InvariantCultureIgnoreCase");

        protected static readonly ImmutableArray<string> CurrentCulture = ImmutableArray.Create(
            "CurrentCultureIgnoreCase");

        protected static readonly ImmutableArray<string> Ordinal = ImmutableArray.Create(
            "OrdinalIgnoreCase");

        /// <summary>
        /// A list of diagnostic IDs that this provider can provider fixes for.
        /// </summary>
        /// <value>The fixable diagnostic ids.</value>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DoNotCreateStringsForComparisonAnalyzer.RuleId);


        /// <summary>
        /// Gets an optional <see cref="FixAllProvider" /> that can fix all/multiple occurrences of diagnostics fixed by this code fix provider.
        /// Return null if the provider doesn't support fix all/multiple occurrences.
        /// Otherwise, you can return any of the well known fix all providers from <see cref="WellKnownFixAllProviders" /> or implement your own fix all provider.
        /// </summary>
        /// <returns>FixAllProvider.</returns>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            var properties = context.Diagnostics[0].Properties;

            if (properties.TryGetValue(DoNotCreateStringsForComparisonAnalyzer.OperationKey, out var operationKey))
            {
                switch (operationKey)
                {
                    case DoNotCreateStringsForComparisonAnalyzer.OperationBinary:
                        {
                            if (TryGetReplacementSyntaxForBinaryOperation(node, out var leftNode, out var rightnode, out var stringComparisons))
                            {
                                var shouldNegateKey = properties.ContainsKey(DoNotCreateStringsForComparisonAnalyzer.ShouldNegateKey);

                                foreach (var stringComparison in stringComparisons)
                                {
                                    context.RegisterCodeFix(
                                        new DoNotCreateStringsForComparisonStaticWithoutComparisonCodeAction(context.Document, node, leftNode, rightnode, stringComparison, shouldNegateKey),
                                        context.Diagnostics);
                                }
                            }

                            break;
                        }
                    case DoNotCreateStringsForComparisonAnalyzer.OperationSwitch:
                        {
                            // No fix available.

                            break;
                        }
                    case DoNotCreateStringsForComparisonAnalyzer.OperationEqualsInstanceWithoutComparison:
                        {
                            if (TryGetReplacementSyntaxForEqualsInstanceWithoutComparisonOperation(node, out var leftNode, out var rightnode, out var stringComparisons))
                            {
                                foreach (var stringComparison in stringComparisons)
                                {
                                    context.RegisterCodeFix(
                                        new DoNotCreateStringsForComparisonInstanceWithoutComparisonCodeAction(context.Document, node, leftNode, rightnode, stringComparison, false),
                                        context.Diagnostics);
                                }
                            }

                            break;
                        }
                    case DoNotCreateStringsForComparisonAnalyzer.OperationEqualsInstanceWithComparison:
                        {
                            if (TryGetReplacementSyntaxForEqualsInstanceWithComparisonOperation(node, out var leftNode, out var rightnode, out var stringComparison))
                            {
                                context.RegisterCodeFix(
                                    new DoNotCreateStringsForComparisonInstanceWithComparisonCodeAction(context.Document, node, leftNode, rightnode, stringComparison, false),
                                    context.Diagnostics);
                            }

                            break;
                        }
                    case DoNotCreateStringsForComparisonAnalyzer.OperationEqualsStaticWithoutComparison:
                        {
                            if (TryGetReplacementSyntaxForEqualsStaticWithoutComparisonOperation(node, out var leftNode, out var rightnode, out var stringComparisons))
                            {
                                foreach (var stringComparison in stringComparisons)
                                {
                                    context.RegisterCodeFix(
                                        new DoNotCreateStringsForComparisonStaticWithoutComparisonCodeAction(context.Document, node, leftNode, rightnode, stringComparison, false),
                                        context.Diagnostics);
                                }
                            }

                            break;
                        }
                    case DoNotCreateStringsForComparisonAnalyzer.OperationEqualsStaticWithComparison:
                        {
                            if (TryGetReplacementSyntaxForEqualsStaticWithComparisonOperation(node, out var leftNode, out var rightnode, out var stringComparison))
                            {
                                context.RegisterCodeFix(
                                    new DoNotCreateStringsForComparisonStaticWithComparisonCodeAction(context.Document, node, leftNode, rightnode, stringComparison, false),
                                    context.Diagnostics);
                            }

                            break;
                        }
                }
            }
        }

        protected abstract bool TryGetReplacementSyntaxForBinaryOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightnode, out ImmutableArray<string> stringComparisons);
        protected abstract bool TryGetReplacementSyntaxForEqualsInstanceWithComparisonOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightnode, out SyntaxNode comparisonNode);
        protected abstract bool TryGetReplacementSyntaxForEqualsInstanceWithoutComparisonOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightnode, out ImmutableArray<string> stringComparisons);
        protected abstract bool TryGetReplacementSyntaxForEqualsStaticWithComparisonOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightnode, out SyntaxNode comparisonNode);
        protected abstract bool TryGetReplacementSyntaxForEqualsStaticWithoutComparisonOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightnode, out ImmutableArray<string> stringComparisons);

        private abstract class DoNotCreateStringsForComparisonCodeAction : CodeAction
        {
            private readonly Document document;
            private readonly SyntaxNode node;
            private readonly SyntaxNode leftNode;
            private readonly SyntaxNode rightnode;
            private readonly bool negate;

            protected DoNotCreateStringsForComparisonCodeAction(
                Document document,
                SyntaxNode node,
                SyntaxNode leftNode,
                SyntaxNode rightnode,
                bool negate)
            {
                this.document = document;
                this.node = node;
                this.leftNode = leftNode;
                this.rightnode = rightnode;
                this.negate = negate;
            }

            public sealed override string Title { get; } = MicrosoftNetCoreAnalyzersResources.DoNotCreateStringsForComparisonTitle;

            protected sealed override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var editor = await DocumentEditor.CreateAsync(this.document, cancellationToken).ConfigureAwait(false);
                var generator = editor.Generator;

                var leftNode = this.leftNode;
                var rightnode = this.rightnode;
                var replacementSyntax = GetReplacementSyntax(editor, generator, leftNode, rightnode);

                if (this.negate)
                {
                    replacementSyntax = generator.LogicalNotExpression(replacementSyntax);
                }

                replacementSyntax = replacementSyntax
                    .WithAdditionalAnnotations(Formatter.Annotation)
                    .WithTriviaFrom(this.node);

                editor.ReplaceNode(this.node, replacementSyntax);

                return editor.GetChangedDocument();
            }

            protected abstract SyntaxNode GetReplacementSyntax(DocumentEditor editor, SyntaxGenerator generator, SyntaxNode leftNode, SyntaxNode rightnode);
        }

        private sealed class DoNotCreateStringsForComparisonStaticWithoutComparisonCodeAction
            : DoNotCreateStringsForComparisonCodeAction
        {
            private readonly string stringComparison;

            public DoNotCreateStringsForComparisonStaticWithoutComparisonCodeAction(Document document, SyntaxNode node, SyntaxNode leftNode, SyntaxNode rightnode, string stringComparison, bool negate)
                : base(document, node, leftNode, rightnode, negate)
            {
                this.stringComparison = stringComparison;
                this.EquivalenceKey = stringComparison;
            }

            public override string EquivalenceKey { get; }

            protected sealed override SyntaxNode GetReplacementSyntax(DocumentEditor editor, SyntaxGenerator generator, SyntaxNode leftNode, SyntaxNode rightnode)
            {
                return generator.InvocationExpression(
                    generator.MemberAccessExpression(generator.TypeExpression(SpecialType.System_String), nameof(string.Equals)),
                    leftNode.WithoutLeadingTrivia(),
                    rightnode.WithoutTrailingTrivia(),
                    generator.MemberAccessExpression(generator.TypeExpression(WellKnownTypes.StringComparison(editor.SemanticModel.Compilation)), stringComparison));
            }
        }

        private sealed class DoNotCreateStringsForComparisonStaticWithComparisonCodeAction
            : DoNotCreateStringsForComparisonCodeAction
        {
            private readonly SyntaxNode stringComparison;

            public DoNotCreateStringsForComparisonStaticWithComparisonCodeAction(Document document, SyntaxNode node, SyntaxNode leftNode, SyntaxNode rightnode, SyntaxNode stringComparison, bool negate)
                : base(document, node, leftNode, rightnode, negate)
            {
                this.stringComparison = stringComparison;
                this.EquivalenceKey = stringComparison.ToString();
            }

            public override string EquivalenceKey { get; }

            protected sealed override SyntaxNode GetReplacementSyntax(DocumentEditor editor, SyntaxGenerator generator, SyntaxNode leftNode, SyntaxNode rightnode)
            {
                return generator.InvocationExpression(
                    generator.MemberAccessExpression(generator.TypeExpression(SpecialType.System_String), nameof(string.Equals)),
                    leftNode.WithoutLeadingTrivia(),
                    rightnode.WithoutTrailingTrivia(),
                    this.stringComparison);
            }
        }

        private sealed class DoNotCreateStringsForComparisonInstanceWithoutComparisonCodeAction
            : DoNotCreateStringsForComparisonCodeAction
        {
            private readonly string stringComparison;

            public DoNotCreateStringsForComparisonInstanceWithoutComparisonCodeAction(Document document, SyntaxNode node, SyntaxNode leftNode, SyntaxNode rightnode, string stringComparison, bool negate)
                : base(document, node, leftNode, rightnode, negate)
            {
                this.stringComparison = stringComparison;
                this.EquivalenceKey = stringComparison;
            }

            public override string EquivalenceKey { get; }

            protected sealed override SyntaxNode GetReplacementSyntax(DocumentEditor editor, SyntaxGenerator generator, SyntaxNode leftNode, SyntaxNode rightnode)
            {
                return generator.InvocationExpression(
                    generator.MemberAccessExpression(leftNode.WithoutLeadingTrivia(), nameof(string.Equals)),
                    rightnode.WithoutTrailingTrivia(),
                    generator.MemberAccessExpression(generator.TypeExpression(WellKnownTypes.StringComparison(editor.SemanticModel.Compilation)), stringComparison));
            }
        }

        private sealed class DoNotCreateStringsForComparisonInstanceWithComparisonCodeAction
            : DoNotCreateStringsForComparisonCodeAction
        {
            private readonly SyntaxNode stringComparison;

            public DoNotCreateStringsForComparisonInstanceWithComparisonCodeAction(Document document, SyntaxNode node, SyntaxNode leftNode, SyntaxNode rightnode, SyntaxNode stringComparison, bool negate)
                : base(document, node, leftNode, rightnode, negate)
            {
                this.stringComparison = stringComparison;
                this.EquivalenceKey = stringComparison.ToString();
            }

            public override string EquivalenceKey { get; }

            protected sealed override SyntaxNode GetReplacementSyntax(DocumentEditor editor, SyntaxGenerator generator, SyntaxNode leftNode, SyntaxNode rightnode)
            {
                return generator.InvocationExpression(
                    generator.MemberAccessExpression(leftNode.WithoutLeadingTrivia(), nameof(string.Equals)),
                    rightnode.WithoutTrailingTrivia(),
                    stringComparison);
            }
        }
    }
}
