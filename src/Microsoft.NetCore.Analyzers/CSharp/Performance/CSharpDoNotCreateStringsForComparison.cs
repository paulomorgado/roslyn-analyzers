// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotCreateStringsForComparisonFixer
        : DoNotCreateStringsForComparisonFixer
    {
        protected sealed override bool TryGetReplacementSyntaxForBinaryOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightNode, out ImmutableArray<string> stringComparisons)
        {
            if (node is BinaryExpressionSyntax binaryExpression)
            {
                GetCaseChangingInvocation(binaryExpression.Left, out leftNode, out var leftStringComparisons);
                GetCaseChangingInvocation(binaryExpression.Right, out rightNode, out var rightStringComparisons);

                stringComparisons = leftStringComparisons.Intersect(rightStringComparisons).ToImmutableArray();

                return true;
            }

            leftNode = default;
            rightNode = default;
            stringComparisons = ImmutableArray<string>.Empty;

            return false;
        }

        protected sealed override bool TryGetReplacementSyntaxForEqualsInstanceWithComparisonOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightNode, out SyntaxNode comparisonNode)
        {
            if (node is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                GetCaseChangingInvocation(memberAccessExpression.Expression, out leftNode);
                GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments[0].Expression, out rightNode);

                comparisonNode = invocationExpression.ArgumentList.Arguments[1].Expression;

                return true;
            }

            leftNode = default;
            rightNode = default;
            comparisonNode = null;

            return false;
        }

        protected sealed override bool TryGetReplacementSyntaxForEqualsInstanceWithoutComparisonOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightNode, out ImmutableArray<string> stringComparisons)
        {
            if (node is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                GetCaseChangingInvocation(memberAccessExpression.Expression, out leftNode, out var leftStringComparisons);
                GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments[0].Expression, out rightNode, out var rightStringComparisons);

                stringComparisons = leftStringComparisons.Intersect(rightStringComparisons).ToImmutableArray();

                return true;
            }

            leftNode = default;
            rightNode = default;
            stringComparisons = ImmutableArray<string>.Empty;

            return false;
        }

        protected sealed override bool TryGetReplacementSyntaxForEqualsStaticWithComparisonOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightNode, out SyntaxNode comparisonNode)
        {
            if (node is InvocationExpressionSyntax invocationExpression)
            {
                GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments[0].Expression, out leftNode);
                GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments[1].Expression, out rightNode);

                comparisonNode = invocationExpression.ArgumentList.Arguments[2].Expression;

                return true;
            }

            leftNode = default;
            rightNode = default;
            comparisonNode = null;

            return false;
        }

        protected sealed override bool TryGetReplacementSyntaxForEqualsStaticWithoutComparisonOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightNode, out ImmutableArray<string> stringComparisons)
        {
            if (node is InvocationExpressionSyntax invocationExpression)
            {
                GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments[0].Expression, out leftNode, out var leftStringComparisons);
                GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments[1].Expression, out rightNode, out var rightStringComparisons);

                stringComparisons = leftStringComparisons.Intersect(rightStringComparisons).ToImmutableArray();

                return true;
            }

            leftNode = default;
            rightNode = default;
            stringComparisons = ImmutableArray<string>.Empty;

            return false;
        }

        private static void GetCaseChangingInvocation(SyntaxNode node, out SyntaxNode expression, out ImmutableArray<string> stringComparisons)
        {
            if (node is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                switch (memberAccessExpression.Name.Identifier.ValueText)
                {
                    case DoNotCreateStringsForComparisonAnalyzer.ToLowerInvariantCultureCaseChangingMethodName:
                    case DoNotCreateStringsForComparisonAnalyzer.ToUpperInvariantCultureCaseChangingMethodName:

                        expression = memberAccessExpression.Expression;
                        stringComparisons = InvariantCulture;

                        return;

                    case DoNotCreateStringsForComparisonAnalyzer.ToLowerCurrentCultureCaseChangingMethodName:
                    case DoNotCreateStringsForComparisonAnalyzer.ToUpperCurrentCultureCaseChangingMethodName:

                        expression = memberAccessExpression.Expression;
                        stringComparisons = GetComparisonsFromArguments(invocationExpression.ArgumentList);

                        return;
                }
            }

            expression = node;
            stringComparisons = All;
        }

        private static void GetCaseChangingInvocation(SyntaxNode node, out SyntaxNode expression)
        {
            if (node is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                switch (memberAccessExpression.Name.Identifier.ValueText)
                {
                    case DoNotCreateStringsForComparisonAnalyzer.ToLowerInvariantCultureCaseChangingMethodName:
                    case DoNotCreateStringsForComparisonAnalyzer.ToUpperInvariantCultureCaseChangingMethodName:
                    case DoNotCreateStringsForComparisonAnalyzer.ToLowerCurrentCultureCaseChangingMethodName:
                    case DoNotCreateStringsForComparisonAnalyzer.ToUpperCurrentCultureCaseChangingMethodName:

                        expression = memberAccessExpression.Expression;

                        return;
                }
            }

            expression = node;
        }

        private static ImmutableArray<string> GetComparisonsFromArguments(ArgumentListSyntax arguments)
        {
            if (arguments.Arguments.Count == 0)
            {
                return All;
            }

            if (arguments.Arguments.Count == 1)
            {
                var argument = arguments.Arguments[0];

                if (argument.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    switch (memberAccess.Name.Identifier.ValueText)
                    {
                        case nameof(CultureInfo.CurrentCulture): return CurrentCulture;
                        case nameof(CultureInfo.InvariantCulture): return InvariantCulture;
                    }
                }

            }

            return ImmutableArray<string>.Empty;
        }
    }
}