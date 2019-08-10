﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Operations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpUsePropertyInsteadOfCountMethodWhenAvailableFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicUsePropertyInsteadOfCountMethodWhenAvailableFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public static partial class UsePropertyInsteadOfCountMethodWhenAvailableTests
    {
        [Theory]
        [InlineData("string[]", nameof(Array.Length))]
        [InlineData("System.Collections.Immutable.ImmutableArray<int>", nameof(ImmutableArray<int>.Length))]
        [InlineData("System.Collections.Generic.List<int>", nameof(List<int>.Count))]
        [InlineData("System.Collections.Generic.IList<int>", nameof(IList<int>.Count))]
        [InlineData("System.Collections.Generic.ICollection<int>", nameof(ICollection<int>.Count))]
        public static Task CSharp_Fixed(string type, string propertyName)
            => VerifyCS.VerifyCodeFixAsync(
                $@"using System;
using System.Linq;
public static class C
{{
    public static {type} GetData() => default;
    public static int M() => GetData().Count();
}}
",
                VerifyCS.Diagnostic(UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.RuleId)
                    .WithLocation(6, 30)
                    .WithArguments(propertyName),
                $@"using System;
using System.Linq;
public static class C
{{
    public static {type} GetData() => default;
    public static int M() => GetData().{propertyName};
}}
");

        [Theory]
        [InlineData("string()", nameof(Array.Length))]
        [InlineData("System.Collections.Immutable.ImmutableArray(Of Integer)", nameof(ImmutableArray<int>.Length))]
        public static Task Basic_Fixed(string type, string propertyName)
            => VerifyVB.VerifyCodeFixAsync(
                $@"Imports System
Imports System.Linq
Public Module M
    Public Function GetData() As {type}
        Return Nothing
    End Function
    Public Function F() As Integer
        Return GetData().Count()
    End Function
End Module
",
                VerifyCS.Diagnostic(UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.RuleId)
                    .WithLocation(8, 16)
                    .WithArguments(propertyName),
                $@"Imports System
Imports System.Linq
Public Module M
    Public Function GetData() As {type}
        Return Nothing
    End Function
    Public Function F() As Integer
        Return GetData().{propertyName}
    End Function
End Module
");

        [Theory]
        [InlineData("System.Collections.Generic.IEnumerable<int>")]
        public static Task CSharp_NoDiagnostic(string type)
            => VerifyCS.VerifyAnalyzerAsync(
                $@"using System;
using System.Linq;
public static class C
{{
    public static {type} GetData() => default;
    public static int M() => GetData().Count();
}}
");

        [Theory]
        [InlineData("System.Collections.Generic.List(Of Integer)")]
        [InlineData("System.Collections.Generic.IList(Of Integer)")]
        [InlineData("System.Collections.Generic.ICollection(Of Integer)")]
        [InlineData("System.Collections.Generic.IEnumerable(Of Integer)")]
        public static Task Basic_NoDiagnostic(string type)
            => VerifyVB.VerifyAnalyzerAsync(
                $@"Imports System
Imports System.Linq
Public Module M
    Public Function GetData() As {type}
        Return Nothing
    End Function
    Public Function F() As Integer
        Return GetData().Count()
    End Function
End Module
");

        [Fact]
        public static Task CSharp_ICollectionOfTImplementerWithImplicitCount_Fixed()
            => VerifyCS.VerifyCodeFixAsync(
                $@"using System;
using System.Linq;
public class T : global::System.Collections.Generic.ICollection<string>
{{
    public int Count => throw new NotImplementedException();
    public bool IsReadOnly => throw new NotImplementedException();
    public void Add(string item) => throw new NotImplementedException();
    public void Clear() => throw new NotImplementedException();
    public bool Contains(string item) => throw new NotImplementedException();
    public void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
    public global::System.Collections.Generic.IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
    public bool Remove(string item) => throw new NotImplementedException();
    global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
}}
public static class C
{{
    public static T GetData() => default;
    public static int M() => GetData().Count();
}}
",
                VerifyCS.Diagnostic(UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.RuleId)
                    .WithLocation(18, 30)
                    .WithArguments("Count"),
                $@"using System;
using System.Linq;
public class T : global::System.Collections.Generic.ICollection<string>
{{
    public int Count => throw new NotImplementedException();
    public bool IsReadOnly => throw new NotImplementedException();
    public void Add(string item) => throw new NotImplementedException();
    public void Clear() => throw new NotImplementedException();
    public bool Contains(string item) => throw new NotImplementedException();
    public void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
    public global::System.Collections.Generic.IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
    public bool Remove(string item) => throw new NotImplementedException();
    global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
}}
public static class C
{{
    public static T GetData() => default;
    public static int M() => GetData().Count;
}}
");

        [Fact]
        public static Task CSharp_ICollectionImplementerWithImplicitCount_Fixed()
            => VerifyCS.VerifyCodeFixAsync(
                $@"using System;
using System.Linq;
public class T :
    global::System.Collections.Generic.IEnumerable<string>,
    global::System.Collections.ICollection
{{
    public int Count => throw new NotImplementedException();
    bool global::System.Collections.ICollection.IsSynchronized => throw new NotImplementedException();
    object global::System.Collections.ICollection.SyncRoot => throw new NotImplementedException();
    void global::System.Collections.ICollection.CopyTo(global::System.Array array, int arrayIndex) => throw new NotImplementedException();
    public global::System.Collections.Generic.IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
    global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
}}
public static class C
{{
    public static T GetData() => default;
    public static int M() => GetData().Count();
}}
",
                VerifyCS.Diagnostic(UsePropertyInsteadOfCountMethodWhenAvailableAnalyzer.RuleId)
                    .WithLocation(17, 30)
                    .WithArguments("Count"),
                $@"using System;
using System.Linq;
public class T :
    global::System.Collections.Generic.IEnumerable<string>,
    global::System.Collections.ICollection
{{
    public int Count => throw new NotImplementedException();
    bool global::System.Collections.ICollection.IsSynchronized => throw new NotImplementedException();
    object global::System.Collections.ICollection.SyncRoot => throw new NotImplementedException();
    void global::System.Collections.ICollection.CopyTo(global::System.Array array, int arrayIndex) => throw new NotImplementedException();
    public global::System.Collections.Generic.IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
    global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
}}
public static class C
{{
    public static T GetData() => default;
    public static int M() => GetData().Count;
}}
");

        [Fact]
        public static Task CSharp_ICollectionOfTImplementerWithExplicitCount_NoDiagnostic()
            => VerifyCS.VerifyAnalyzerAsync(
                $@"using System;
using System.Linq;
public class T : global::System.Collections.Generic.ICollection<string>
{{
    int global::System.Collections.Generic.ICollection<string>.Count => throw new NotImplementedException();
    bool global::System.Collections.Generic.ICollection<string>.IsReadOnly => throw new NotImplementedException();
    void global::System.Collections.Generic.ICollection<string>.Add(string item) => throw new NotImplementedException();
    void global::System.Collections.Generic.ICollection<string>.Clear() => throw new NotImplementedException();
    bool global::System.Collections.Generic.ICollection<string>.Contains(string item) => throw new NotImplementedException();
    void global::System.Collections.Generic.ICollection<string>.CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
    public global::System.Collections.Generic.IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
    global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
    bool global::System.Collections.Generic.ICollection<string>.Remove(string item) => throw new NotImplementedException();
}}
public static class C
{{
    public static T GetData() => default;
    public static int M() => GetData().Count();
}}
");

        [Fact]
        public static Task CSharp_InterfaceShadowingICollectionOfT_NoDiagnostic()
            => VerifyCS.VerifyAnalyzerAsync(
                @"using System;
using System.Linq;
public interface I : global::System.Collections.Generic.ICollection<string>
{
    new int Count { get; }
}
public static class C
{
    public static I GetData() => default;
    public static int M() => GetData().Count();
}
");

        [Fact]
        public static Task CSharp_InterfaceShadowingICollection_NoDiagnostic()
            => VerifyCS.VerifyAnalyzerAsync(
                @"using System;
using System.Linq;
public interface I :
    global::System.Collections.Generic.IEnumerable<string>,
    global::System.Collections.ICollection
{
    new int Count { get; }
}
public static class C
{
    public static I GetData() => default;
    public static int M() => GetData().Count();
}
");

        [Fact]
        public static Task CSharp_ClassShadowingICollectionOfT_NoDiagnostic()
            => VerifyCS.VerifyAnalyzerAsync(
                $@"using System;
using System.Linq;
public class T : global::System.Collections.Generic.ICollection<string>
{{
    public int Count => throw new NotImplementedException();
    int global::System.Collections.Generic.ICollection<string>.Count => throw new NotImplementedException();
    bool global::System.Collections.Generic.ICollection<string>.IsReadOnly => throw new NotImplementedException();
    void global::System.Collections.Generic.ICollection<string>.Add(string item) => throw new NotImplementedException();
    void global::System.Collections.Generic.ICollection<string>.Clear() => throw new NotImplementedException();
    bool global::System.Collections.Generic.ICollection<string>.Contains(string item) => throw new NotImplementedException();
    void global::System.Collections.Generic.ICollection<string>.CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
    bool global::System.Collections.Generic.ICollection<string>.Remove(string item) => throw new NotImplementedException();
    public global::System.Collections.Generic.IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
    global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
}}
public static class C
{{
    public static T GetData() => default;
    public static int M() => GetData().Count();
}}
");

        [Fact]
        public static Task CSharp_ClassShadowingICollection_NoDiagnostic()
            => VerifyCS.VerifyAnalyzerAsync(
                $@"using System;
using System.Linq;
public class T :
    global::System.Collections.Generic.IEnumerable<string>,
    global::System.Collections.ICollection
{{
    public int Count => throw new NotImplementedException();
    int global::System.Collections.ICollection.Count => throw new NotImplementedException();
    bool global::System.Collections.ICollection.IsSynchronized => throw new NotImplementedException();
    object global::System.Collections.ICollection.SyncRoot => throw new NotImplementedException();
    void global::System.Collections.ICollection.CopyTo(global::System.Array array, int arrayIndex) => throw new NotImplementedException();
    public global::System.Collections.Generic.IEnumerator<string> GetEnumerator() => throw new NotImplementedException();
    global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
}}
public static class C
{{
    public static T GetData() => default;
    public static int M() => GetData().Count();
}}
");
    }
}
