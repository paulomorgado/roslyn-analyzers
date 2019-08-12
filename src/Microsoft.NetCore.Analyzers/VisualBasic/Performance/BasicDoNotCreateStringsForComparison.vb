' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.NetCore.Analyzers.Performance
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports System.Collections.Immutable
Imports System.Globalization

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotCreateStringsForComparisonFixer
        Inherits DoNotCreateStringsForComparisonFixer

        Protected Overrides Function TryGetReplacementSyntaxForBinaryOperation(node As SyntaxNode, ByRef leftNode As SyntaxNode, ByRef rightNode As SyntaxNode, ByRef stringComparisons As ImmutableArray(Of String)) As Boolean

            Dim binaryExpression = TryCast(node, BinaryExpressionSyntax)

            If Not binaryExpression Is Nothing Then

                Dim leftStringComparisons As ImmutableArray(Of String) = Nothing
                Dim rightStringComparisons As ImmutableArray(Of String) = Nothing

                GetCaseChangingInvocation(binaryExpression.Left, leftNode, leftStringComparisons)
                GetCaseChangingInvocation(binaryExpression.Right, rightNode, rightStringComparisons)

                stringComparisons = leftStringComparisons.Intersect(rightStringComparisons).ToImmutableArray()

                Return True

            End If

            Return False

        End Function

        Protected Overrides Function TryGetReplacementSyntaxForEqualsInstanceWithComparisonOperation(node As SyntaxNode, ByRef leftNode As SyntaxNode, ByRef rightNode As SyntaxNode, ByRef comparisonNode As SyntaxNode) As Boolean

            Dim invocationExpression = TryCast(node, InvocationExpressionSyntax)

            If Not invocationExpression Is Nothing Then

                Dim memberAccessExpression = TryCast(invocationExpression.Expression, MemberAccessExpressionSyntax)

                If Not memberAccessExpression Is Nothing Then

                    GetCaseChangingInvocation(memberAccessExpression.Expression, leftNode)
                    GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments(0).GetExpression(), rightNode)

                    comparisonNode = invocationExpression.ArgumentList.Arguments(1).GetExpression()

                    Return True

                End If

            End If

            Return False

        End Function

        Protected Overrides Function TryGetReplacementSyntaxForEqualsInstanceWithoutComparisonOperation(node As SyntaxNode, ByRef leftNode As SyntaxNode, ByRef rightnode As SyntaxNode, ByRef stringComparisons As ImmutableArray(Of String)) As Boolean

            Dim invocationExpression = TryCast(node, InvocationExpressionSyntax)

            If Not invocationExpression Is Nothing Then

                Dim memberAccessExpression = TryCast(invocationExpression.Expression, MemberAccessExpressionSyntax)

                If Not memberAccessExpression Is Nothing Then

                    Dim leftStringComparisons As ImmutableArray(Of String) = Nothing
                    Dim rightStringComparisons As ImmutableArray(Of String) = Nothing

                    GetCaseChangingInvocation(memberAccessExpression.Expression, leftNode, leftStringComparisons)
                    GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments(0).GetExpression(), rightnode, rightStringComparisons)

                    stringComparisons = leftStringComparisons.Intersect(rightStringComparisons).ToImmutableArray()

                    Return True

                End If

            End If

            Return False

        End Function

        Protected Overrides Function TryGetReplacementSyntaxForEqualsStaticWithComparisonOperation(node As SyntaxNode, ByRef leftNode As SyntaxNode, ByRef rightnode As SyntaxNode, ByRef comparisonNode As SyntaxNode) As Boolean

            Dim invocationExpression = TryCast(node, InvocationExpressionSyntax)

            If Not invocationExpression Is Nothing Then

                Dim memberAccessExpression = TryCast(invocationExpression.Expression, MemberAccessExpressionSyntax)

                If Not memberAccessExpression Is Nothing Then

                    GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments(0).GetExpression(), leftNode)
                    GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments(1).GetExpression(), rightnode)

                    comparisonNode = invocationExpression.ArgumentList.Arguments(2).GetExpression()

                    Return True

                End If

            End If

            Return False

        End Function

        Protected Overrides Function TryGetReplacementSyntaxForEqualsStaticWithoutComparisonOperation(node As SyntaxNode, ByRef leftNode As SyntaxNode, ByRef rightnode As SyntaxNode, ByRef stringComparisons As ImmutableArray(Of String)) As Boolean

            Dim invocationExpression = TryCast(node, InvocationExpressionSyntax)

            If Not invocationExpression Is Nothing Then

                Dim memberAccessExpression = TryCast(invocationExpression.Expression, MemberAccessExpressionSyntax)

                If Not memberAccessExpression Is Nothing Then

                    Dim leftStringComparisons As ImmutableArray(Of String) = Nothing
                    Dim rightStringComparisons As ImmutableArray(Of String) = Nothing

                    GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments(0).GetExpression(), leftNode, leftStringComparisons)
                    GetCaseChangingInvocation(invocationExpression.ArgumentList.Arguments(1).GetExpression(), rightnode, rightStringComparisons)

                    stringComparisons = leftStringComparisons.Intersect(rightStringComparisons).ToImmutableArray()

                    Return True

                End If

            End If

            Return False

        End Function

        Private Shared Sub GetCaseChangingInvocation(node As SyntaxNode, ByRef expression As SyntaxNode, ByRef stringComparisons As ImmutableArray(Of String))

            Dim invocationExpression = TryCast(node, InvocationExpressionSyntax)

            If Not invocationExpression Is Nothing Then

                Dim memberAccessExpression = TryCast(invocationExpression.Expression, MemberAccessExpressionSyntax)

                If Not memberAccessExpression Is Nothing Then

                    Select Case memberAccessExpression.Name.Identifier.ValueText

                        Case DoNotCreateStringsForComparisonAnalyzer.ToLowerInvariantCultureCaseChangingMethodName,
                             DoNotCreateStringsForComparisonAnalyzer.ToUpperInvariantCultureCaseChangingMethodName

                            expression = memberAccessExpression.Expression
                            stringComparisons = InvariantCulture

                            Return

                        Case DoNotCreateStringsForComparisonAnalyzer.ToLowerCurrentCultureCaseChangingMethodName,
                             DoNotCreateStringsForComparisonAnalyzer.ToUpperCurrentCultureCaseChangingMethodName

                            expression = memberAccessExpression.Expression
                            stringComparisons = GetComparisonsFromArguments(invocationExpression.ArgumentList)

                            Return

                    End Select

                End If

            End If

            expression = node
            stringComparisons = All

        End Sub

        Private Shared Sub GetCaseChangingInvocation(node As SyntaxNode, ByRef expression As SyntaxNode)

            Dim invocationExpression = TryCast(node, InvocationExpressionSyntax)

            If Not invocationExpression Is Nothing Then

                Dim memberAccessExpression = TryCast(invocationExpression.Expression, MemberAccessExpressionSyntax)

                If Not memberAccessExpression Is Nothing Then

                    Select Case memberAccessExpression.Name.Identifier.ValueText

                        Case DoNotCreateStringsForComparisonAnalyzer.ToLowerInvariantCultureCaseChangingMethodName,
                             DoNotCreateStringsForComparisonAnalyzer.ToUpperInvariantCultureCaseChangingMethodName,
                             DoNotCreateStringsForComparisonAnalyzer.ToLowerCurrentCultureCaseChangingMethodName,
                             DoNotCreateStringsForComparisonAnalyzer.ToUpperCurrentCultureCaseChangingMethodName

                            expression = memberAccessExpression.Expression

                            Return

                    End Select

                End If

            End If

            expression = node

        End Sub

        Private Shared Function GetComparisonsFromArguments(argumentList As ArgumentListSyntax) As ImmutableArray(Of String)

            If argumentList.Arguments.Count = 0 Then

                Return All

            End If

            If argumentList.Arguments.Count = 1 Then

                Dim argument = argumentList.Arguments(0)
                Dim memberAccess = TryCast(argument.GetExpression(), MemberAccessExpressionSyntax)

                If Not memberAccess Is Nothing Then

                    Select Case memberAccess.Name.Identifier.ValueText

                        Case NameOf(CultureInfo.CurrentCulture)
                            Return CurrentCulture
                        Case NameOf(CultureInfo.InvariantCulture)
                            Return InvariantCulture

                    End Select

                End If

            End If

            Return ImmutableArray(Of String).Empty

        End Function

    End Class

End Namespace