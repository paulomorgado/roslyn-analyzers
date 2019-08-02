' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.NetCore.Analyzers.Performance
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports System.Collections.Immutable

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Performance
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public NotInheritable Class BasicDoNotCreateStringsForComparisonFixer
        Inherits DoNotCreateStringsForComparisonFixer

        Protected Overrides Function TryGetReplacementSyntax(node As SyntaxNode, ByRef leftNode As SyntaxNode, ByRef rightnode As SyntaxNode, ByRef stringComparisons As ImmutableArray(Of String), ByRef negate As Boolean) As Boolean
            Return False
        End Function

    End Class
End Namespace