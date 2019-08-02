// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Performance;

namespace Microsoft.NetCore.CSharp.Analyzers.Performance
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotCreateStringsForComparisonFixer
        : DoNotCreateStringsForComparisonFixer
    {
        protected override bool TryGetReplacementSyntax(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightnode, out ImmutableArray<string> stringComparisons, out bool negate)
        {
            switch (node)
            {
                case BinaryExpressionSyntax binaryExpression:
                    GetCaseChangingInvocation(binaryExpression.Left, out leftNode, out var leftStringComparisons);
                    GetCaseChangingInvocation(binaryExpression.Right, out rightnode, out var rightStringComparisons);

                    stringComparisons = leftStringComparisons.Intersect(rightStringComparisons).ToImmutableArray();
                    negate = binaryExpression.IsKind(SyntaxKind.NotEqualsExpression);

                    return true;
            }

            leftNode = default;
            rightnode = default;
            stringComparisons = ImmutableArray<string>.Empty;
            negate = default;

            return false;
        }

        private static void GetCaseChangingInvocation(SyntaxNode node, out SyntaxNode expression, out ImmutableArray<string> stringComparisons)
        {
            if (node is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                switch (memberAccessExpression.Name.Identifier.ValueText)
                {
                    case DoNotCreateStringsForComparisonAnalyzer.ToLowerInvariantCaseChangingMethodName:
                    case DoNotCreateStringsForComparisonAnalyzer.ToUpperInvariantCaseChangingMethodName:

                        expression = memberAccessExpression.Expression;
                        stringComparisons = InvariantCulture;

                        return;

                    case DoNotCreateStringsForComparisonAnalyzer.ToLowerCaseChangingMethodName:
                    case DoNotCreateStringsForComparisonAnalyzer.ToUpperCaseChangingMethodName:

                        expression = memberAccessExpression.Expression;
                        stringComparisons = GetComparisonsFromArguments(invocationExpression.ArgumentList);

                        return;
                }
            }

            expression = node;
            stringComparisons = All;
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