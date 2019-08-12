// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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
        protected override bool TryGetReplacementSyntaxForBinaryOperation(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightnode, out ImmutableArray<string> stringComparisons)
        {
            if (node is BinaryExpressionSyntax binaryExpression)
            {
                GetCaseChangingInvocation(binaryExpression.Left, out leftNode, out var leftStringComparisons);
                GetCaseChangingInvocation(binaryExpression.Right, out rightnode, out var rightStringComparisons);

                stringComparisons = leftStringComparisons.Intersect(rightStringComparisons).ToImmutableArray();

                return true;
            }

            leftNode = default;
            rightnode = default;
            stringComparisons = ImmutableArray<string>.Empty;

            return false;
        }

        protected override bool TryGetReplacementSyntax(SyntaxNode node, out SyntaxNode leftNode, out SyntaxNode rightnode, out ImmutableArray<string> stringComparisons, out bool negate)
        {
            switch (node)
            {
                case InvocationExpressionSyntax invocationExpression when TryGetEqualsArguments(invocationExpression, out var argument1, out var argument2):
                    {
                        GetCaseChangingInvocation(argument1, out leftNode, out var leftStringComparisons);
                        GetCaseChangingInvocation(argument2, out rightnode, out var rightStringComparisons);

                        stringComparisons = leftStringComparisons.Intersect(rightStringComparisons).ToImmutableArray();
                        negate = false;

                        return true;
                    }
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

        private static bool TryGetEqualsArguments(InvocationExpressionSyntax invocation, out SyntaxNode argument1, out SyntaxNode argument2)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression is PredefinedTypeSyntax ||
                    memberAccess.Expression is MemberAccessExpressionSyntax type && type.Expression is NameSyntax ||
                    memberAccess.Expression is NameSyntax)
                {
                    argument1 = invocation.ArgumentList.Arguments[0].Expression;
                    argument2 = invocation.ArgumentList.Arguments[1].Expression;

                    return true;
                }

                argument1 = memberAccess.Expression;
                argument2 = invocation.ArgumentList.Arguments[0].Expression;

                return true;
            }

            argument1 = default;
            argument2 = default;

            return false;
        }
    }
}