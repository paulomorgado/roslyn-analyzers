// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using Xunit.Abstractions;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DoNotCreateStringsForComparisonAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Performance.CSharpDoNotCreateStringsForComparisonFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Performance.DoNotCreateStringsForComparisonAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Performance.BasicDoNotCreateStringsForComparisonFixer>;

namespace Microsoft.NetCore.Analyzers.Performance.UnitTests
{
    public static class DoNotCreateStringsForComparisonTests
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static TheoryData<string> StringComparison_TheoryData = new TheoryData<string>
        {
            { "OrdinalIgnoreCase" },
            { "InvariantCultureIgnoreCase" },
            { "CurrentCultureIgnoreCase" },
        };

        public static TheoryData<string> ToXInvariant_NonCaseChanging_TheoryData = new TheoryData<string>
        {
            { "Lower" },
            { "Upper" },
        };

        public static TheoryData<string, string> ToXWithCultureInfo_NonCaseChanging_TheoryData = new TheoryData<string, string>
        {
            { nameof(CultureInfo.InvariantCulture), "InvariantCultureIgnoreCase" },
            { nameof(CultureInfo.CurrentCulture), "CurrentCultureIgnoreCase" },
        };

        public static TheoryData<string> Switch_TheoryData = new TheoryData<string>
        {
            { "ToLowerInvariant()" },
            { "ToUpper(System.Globalization.CultureInfo.CurrentCulture)" },
            { "ToUpper(System.Globalization.CultureInfo.InvariantCulture)" },
        };

        public static TheoryData<string, string, string, string, string> MixedEqualsComparison_TheoryData = new TheoryData<string, string, string, string, string>
        {
            { "\"x\"", "ToUpperInvariant()", "\"y\"", "ToLowerInvariant()", "InvariantCultureIgnoreCase" },
            { "\"x\"", "ToUpper()", "\"y\"", "ToLowerInvariant()", "InvariantCultureIgnoreCase" },
            { "\"x\"", "ToUpperInvariant()", "\"y\"", "ToLower()", "InvariantCultureIgnoreCase" },
            { "\"x\"", "ToUpper()", "\"y\"", "ToLower()", "OrdinalIgnoreCase" },
            { "\"x\"", "ToUpper()", "\"y\"", "ToLower()", "InvariantCultureIgnoreCase" },
            { "\"x\"", "ToUpper()", "\"y\"", "ToLower()", "CurrentCultureIgnoreCase" },
        };

        public static TheoryData<string, string, string, string> MixedEqualsComparison_Diagnostic_NoFix_TheoryData = new TheoryData<string, string, string, string>
        {
            { "\"x\"", "ToUpper(System.Globalization.CultureInfo.CurrentCulture)", "\"y\"", "ToLowerInvariant()" },
            { "\"x\"", "ToUpperInvariant()", "\"y\"", "ToLower(System.Globalization.CultureInfo.CurrentCulture)" },
            { "\"x\"", "ToUpper(System.Globalization.CultureInfo.CurrentCulture)", "\"y\"", "ToLower(System.Globalization.CultureInfo.InvariantCulture)" },
            { "\"x\"", "ToUpper(System.Globalization.CultureInfo.InvariantCulture)", "\"y\"", "ToLower(System.Globalization.CultureInfo.CurrentCulture)" },
        };
#pragma warning restore CA2211 // Non-constant fields should not be visible

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task CSharp_ToXComparison(string expectedStringComparison)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|""y"".ToUpper() == ""x""|];
        _ = [|""x"" == ""y"".ToUpper()|];
        _ = [|""y"".ToUpper() != ""x""|];
        _ = [|""x"" != ""y"".ToUpper()|];
        _ = [|""y"".ToLower() == ""x""|];
        _ = [|""x"" == ""y"".ToLower()|];
        _ = [|""y"".ToLower() != ""x""|];
        _ = [|""x"" != ""y"".ToLower()|];

        _ = [|""y""?.ToUpper() == ""x""|];
        _ = [|""x"" == ""y""?.ToUpper()|];
        _ = [|""y""?.ToUpper() != ""x""|];
        _ = [|""x"" != ""y""?.ToUpper()|];
        _ = [|""y""?.ToLower() == ""x""|];
        _ = [|""x"" == ""y""?.ToLower()|];
        _ = [|""y""?.ToLower() != ""x""|];
        _ = [|""x"" != ""y""?.ToLower()|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});

        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task Basic_ToXComparison(string expectedStringComparison)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|""y"".ToUpper() = ""x""|]
        b = [|""x"" = ""y"".ToUpper()|]
        b = [|""y"".ToUpper() <> ""x""|]
        b = [|""x"" <> ""y"".ToUpper()|]
        b = [|""y"".ToLower() = ""x""|]
        b = [|""x"" = ""y"".ToLower()|]
        b = [|""y"".ToLower() <> ""x""|]
        b = [|""x"" <> ""y"".ToLower()|]

        b = [|""y""?.ToUpper() = ""x""|]
        b = [|""x"" = ""y""?.ToUpper()|]
        b = [|""y""?.ToUpper() <> ""x""|]
        b = [|""x"" <> ""y""?.ToUpper()|]
        b = [|""y""?.ToLower() = ""x""|]
        b = [|""x"" = ""y""?.ToLower()|]
        b = [|""y""?.ToLower() <> ""x""|]
        b = [|""x"" <> ""y""?.ToLower()|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})

        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
    End Sub
End Module
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task CSharp_ToXInstanceEquals1(string expectedStringComparison)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|""y"".ToUpper().Equals(""x"")|];
        _ = [|""x"".Equals(""y"".ToUpper())|];
        _ = [|""y"".ToLower().Equals(""x"")|];
        _ = [|""x"".Equals(""y"".ToLower())|];

        _ = [|""y""?.ToUpper().Equals(""x"")|];
        _ = [|""x"".Equals(""y""?.ToUpper())|];
        _ = [|""y""?.ToLower().Equals(""x"")|];
        _ = [|""x"".Equals(""y""?.ToLower())|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = ""y"".Equals(""x"", StringComparison.{expectedStringComparison});
        _ = ""x"".Equals(""y"", StringComparison.{expectedStringComparison});
        _ = ""y"".Equals(""x"", StringComparison.{expectedStringComparison});
        _ = ""x"".Equals(""y"", StringComparison.{expectedStringComparison});

        _ = ""y"".Equals(""x"", StringComparison.{expectedStringComparison});
        _ = ""x"".Equals(""y"", StringComparison.{expectedStringComparison});
        _ = ""y"".Equals(""x"", StringComparison.{expectedStringComparison});
        _ = ""x"".Equals(""y"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task Basic_ToXInstanceEquals1(string expectedStringComparison)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|""y"".ToUpper().Equals(""x"")|]
        b = [|""x"".Equals(""y"".ToUpper())|]
        b = [|""y"".ToLower().Equals(""x"")|]
        b = [|""x"".Equals(""y"".ToLower())|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = ""y"".Equals(""x"", StringComparison.{expectedStringComparison})
        b = ""x"".Equals(""y"", StringComparison.{expectedStringComparison})
        b = ""y"".Equals(""x"", StringComparison.{expectedStringComparison})
        b = ""x"".Equals(""y"", StringComparison.{expectedStringComparison})
    End Sub
End Module
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task CSharp_ToXInstanceEquals2(string expectedStringComparison)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|""y"".ToUpper().Equals(""x"", StringComparison.{expectedStringComparison})|];
        _ = [|""x"".Equals(""y"".ToUpper(), StringComparison.{expectedStringComparison})|];
        _ = [|""y"".ToLower().Equals(""x"", StringComparison.{expectedStringComparison})|];
        _ = [|""x"".Equals(""y"".ToLower(), StringComparison.{expectedStringComparison})|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = ""y"".Equals(""x"", StringComparison.{expectedStringComparison});
        _ = ""x"".Equals(""y"", StringComparison.{expectedStringComparison});
        _ = ""y"".Equals(""x"", StringComparison.{expectedStringComparison});
        _ = ""x"".Equals(""y"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = $"StringComparison.{expectedStringComparison}",
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task Bash_ToXInstanceEquals2(string expectedStringComparison)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|""y"".ToUpper().Equals(""x"", StringComparison.{expectedStringComparison})|]
        b = [|""x"".Equals(""y"".ToUpper(), StringComparison.{expectedStringComparison})|]
        b = [|""y"".ToLower().Equals(""x"", StringComparison.{expectedStringComparison})|]
        b = [|""x"".Equals(""y"".ToLower(), StringComparison.{expectedStringComparison})|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = ""y"".Equals(""x"", StringComparison.{expectedStringComparison})
        b = ""x"".Equals(""y"", StringComparison.{expectedStringComparison})
        b = ""y"".Equals(""x"", StringComparison.{expectedStringComparison})
        b = ""x"".Equals(""y"", StringComparison.{expectedStringComparison})
    End Sub
End Module
",
                    },
                },
                CodeFixEquivalenceKey = $"StringComparison.{expectedStringComparison}",
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task CSharp_ToXStringEqualsStatic2(string expectedStringComparison)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|string.Equals(""y"".ToUpper(), ""x"")|];
        _ = [|string.Equals(""x"", ""y"".ToUpper())|];
        _ = [|string.Equals(""y"".ToLower(), ""x"")|];
        _ = [|string.Equals(""x"", ""y"".ToLower())|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task Basic_ToXStringEqualsStatic2(string expectedStringComparison)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|String.Equals(""y"".ToUpper(), ""x"")|]
        b = [|String.Equals(""x"", ""y"".ToUpper())|]
        b = [|String.Equals(""y"".ToLower(), ""x"")|]
        b = [|String.Equals(""x"", ""y"".ToLower())|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
    End Sub
End Module
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task CSharp_ToXStringEqualsStatic3(string expectedStringComparison)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|string.Equals(""y"".ToUpper(), ""x"", StringComparison.{expectedStringComparison})|];
        _ = [|string.Equals(""x"", ""y"".ToUpper(), StringComparison.{expectedStringComparison})|];
        _ = [|string.Equals(""y"".ToLower(), ""x"", StringComparison.{expectedStringComparison})|];
        _ = [|string.Equals(""x"", ""y"".ToLower(), StringComparison.{expectedStringComparison})|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = $"StringComparison.{expectedStringComparison}",
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task Basic_ToXStringEqualsStatic3(string expectedStringComparison)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|String.Equals(""y"".ToUpper(), ""x"", StringComparison.{expectedStringComparison})|]
        b = [|String.Equals(""x"", ""y"".ToUpper(), StringComparison.{expectedStringComparison})|]
        b = [|String.Equals(""y"".ToLower(), ""x"", StringComparison.{expectedStringComparison})|]
        b = [|String.Equals(""x"", ""y"".ToLower(), StringComparison.{expectedStringComparison})|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
    End Sub
End Module
",
                    },
                },
                CodeFixEquivalenceKey = $"StringComparison.{expectedStringComparison}",
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task CSharp_ToXSystemStringEqualsStatic2(string expectedStringComparison)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|global::System.String.Equals(""y"".ToUpper(), ""x"")|];
        _ = [|System.String.Equals(""x"", ""y"".ToUpper())|];
        _ = [|String.Equals(""y"".ToLower(), ""x"")|];
        _ = [|String.Equals(""x"", ""y"".ToLower())|];

        _ = [|global::System.String.Equals(""y""?.ToUpper(), ""x"")|];
        _ = [|System.String.Equals(""x"", ""y""?.ToUpper())|];
        _ = [|String.Equals(""y""?.ToLower(), ""x"")|];
        _ = [|String.Equals(""x"", ""y""?.ToLower())|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});

        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task Bash_ToXSystemStringEqualsStatic2(string expectedStringComparison)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|Global.System.String.Equals(""y"".ToUpper(), ""x"")|]
        b = [|System.String.Equals(""x"", ""y"".ToUpper())|]
        b = [|String.Equals(""y"".ToLower(), ""x"")|]
        b = [|String.Equals(""x"", ""y"".ToLower())|]

        b = [|Global.System.String.Equals(""y""?.ToUpper(), ""x"")|]
        b = [|System.String.Equals(""x"", ""y""?.ToUpper())|]
        b = [|String.Equals(""y""?.ToLower(), ""x"")|]
        b = [|String.Equals(""x"", ""y""?.ToLower())|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})

        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
    End Sub
End Module
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Fact]
        public static Task CSharp_ToXSystemStringEqualsStatic3()
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        var c = StringComparison.OrdinalIgnoreCase;
        _ = [|global::System.String.Equals(""y"".ToUpper(), ""x"", c)|];
        _ = [|System.String.Equals(""x"", ""y"".ToUpper(), c)|];
        _ = [|String.Equals(""y"".ToLower(), ""x"", c)|];
        _ = [|String.Equals(""x"", ""y"".ToLower(), c)|];

        _ = [|global::System.String.Equals(""y""?.ToUpper(), ""x"", c)|];
        _ = [|System.String.Equals(""x"", ""y""?.ToUpper(), c)|];
        _ = [|String.Equals(""y""?.ToLower(), ""x"", c)|];
        _ = [|String.Equals(""x"", ""y""?.ToLower(), c)|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        var c = StringComparison.OrdinalIgnoreCase;
        _ = string.Equals(""y"", ""x"", c);
        _ = string.Equals(""x"", ""y"", c);
        _ = string.Equals(""y"", ""x"", c);
        _ = string.Equals(""x"", ""y"", c);

        _ = string.Equals(""y"", ""x"", c);
        _ = string.Equals(""x"", ""y"", c);
        _ = string.Equals(""y"", ""x"", c);
        _ = string.Equals(""x"", ""y"", c);
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = "c",
            }.RunAsync();

        [Fact]
        public static Task Bash_ToXSystemStringEqualsStatic3()
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim c = StringComparison.OrdinalIgnoreCase
        Dim b As Boolean
        b = [|Global.System.String.Equals(""y"".ToUpper(), ""x"", c)|]
        b = [|System.String.Equals(""x"", ""y"".ToUpper(), c)|]
        b = [|String.Equals(""y"".ToLower(), ""x"", c)|]
        b = [|String.Equals(""x"", ""y"".ToLower(), c)|]

        b = [|Global.System.String.Equals(""y""?.ToUpper(), ""x"", c)|]
        b = [|System.String.Equals(""x"", ""y""?.ToUpper(), c)|]
        b = [|String.Equals(""y""?.ToLower(), ""x"", c)|]
        b = [|String.Equals(""x"", ""y""?.ToLower(), c)|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim c = StringComparison.OrdinalIgnoreCase
        Dim b As Boolean
        b = String.Equals(""y"", ""x"", c)
        b = String.Equals(""x"", ""y"", c)
        b = String.Equals(""y"", ""x"", c)
        b = String.Equals(""x"", ""y"", c)

        b = String.Equals(""y"", ""x"", c)
        b = String.Equals(""x"", ""y"", c)
        b = String.Equals(""y"", ""x"", c)
        b = String.Equals(""x"", ""y"", c)
    End Sub
End Module
",
                    },
                },
                CodeFixEquivalenceKey = "c",
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToXWithCultureInfo_NonCaseChanging_TheoryData))]
        public static Task CSharp_ToXWithCultureInfoPropertyComparisonNonCaseChanging(string cultureInfo, string expectedStringComparison)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|""y"".ToUpper(System.Globalization.CultureInfo.{cultureInfo}) == ""x""|];
        _ = [|""y"".ToUpper(System.Globalization.CultureInfo.{cultureInfo}) != ""x""|];
        _ = [|""x"" == ""y"".ToUpper(System.Globalization.CultureInfo.{cultureInfo})|];
        _ = [|""x"" != ""y"".ToUpper(System.Globalization.CultureInfo.{cultureInfo})|];
        _ = [|""y"".ToLower(System.Globalization.CultureInfo.{cultureInfo}) == ""x""|];
        _ = [|""y"".ToLower(System.Globalization.CultureInfo.{cultureInfo}) != ""x""|];
        _ = [|""x"" == ""y"".ToLower(System.Globalization.CultureInfo.{cultureInfo})|];
        _ = [|""x"" != ""y"".ToLower(System.Globalization.CultureInfo.{cultureInfo})|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""y"", ""x"", StringComparison.{expectedStringComparison});
        _ = string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
        _ = !string.Equals(""x"", ""y"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToXWithCultureInfo_NonCaseChanging_TheoryData))]
        public static Task Basic_ToXWithCultureInfoPropertyComparisonNonCaseChanging(string cultureInfo, string expectedStringComparison)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|""y"".ToUpper(System.Globalization.CultureInfo.{cultureInfo}) = ""x""|]
        b = [|""y"".ToUpper(System.Globalization.CultureInfo.{cultureInfo}) <> ""x""|]
        b = [|""x"" = ""y"".ToUpper(System.Globalization.CultureInfo.{cultureInfo})|]
        b = [|""x"" <> ""y"".ToUpper(System.Globalization.CultureInfo.{cultureInfo})|]
        b = [|""y"".ToLower(System.Globalization.CultureInfo.{cultureInfo}) = ""x""|]
        b = [|""y"".ToLower(System.Globalization.CultureInfo.{cultureInfo}) <> ""x""|]
        b = [|""x"" = ""y"".ToLower(System.Globalization.CultureInfo.{cultureInfo})|]
        b = [|""x"" <> ""y"".ToLower(System.Globalization.CultureInfo.{cultureInfo})|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""y"", ""x"", StringComparison.{expectedStringComparison})
        b = String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
        b = Not String.Equals(""x"", ""y"", StringComparison.{expectedStringComparison})
    End Sub
End Module
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Fact]
        public static Task CSharp_ToXWithCustomCultureInfoomparisonNonCaseChanging()
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = ""y"".ToUpper(System.Globalization.CultureInfo.GetCultureInfo(""x"")) == ""x"";
        _ = ""y"".ToUpper(System.Globalization.CultureInfo.GetCultureInfo(""x"")) != ""x"";
        _ = ""x"" == ""y"".ToUpper(System.Globalization.CultureInfo.GetCultureInfo(""x""));
        _ = ""x"" != ""y"".ToUpper(System.Globalization.CultureInfo.GetCultureInfo(""x""));
        _ = ""y"".ToLower(System.Globalization.CultureInfo.GetCultureInfo(""x"")) == ""x"";
        _ = ""y"".ToLower(System.Globalization.CultureInfo.GetCultureInfo(""x"")) != ""x"";
        _ = ""x"" == ""y"".ToLower(System.Globalization.CultureInfo.GetCultureInfo(""x""));
        _ = ""x"" != ""y"".ToLower(System.Globalization.CultureInfo.GetCultureInfo(""x""));
    }}
}}
",
                    },
                },
            }.RunAsync();

        [Fact]
        public static Task Basic_ToXWithCustomCultureInfoomparisonNonCaseChanging()
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = ""y"".ToUpper(System.Globalization.CultureInfo.GetCultureInfo(""x"")) = ""x""
        b = ""y"".ToUpper(System.Globalization.CultureInfo.GetCultureInfo(""x"")) <> ""x""
        b = ""x"" = ""y"".ToUpper(System.Globalization.CultureInfo.GetCultureInfo(""x""))
        b = ""x"" <> ""y"".ToUpper(System.Globalization.CultureInfo.GetCultureInfo(""x""))
        b = ""y"".ToLower(System.Globalization.CultureInfo.GetCultureInfo(""x"")) = ""x""
        b = ""y"".ToLower(System.Globalization.CultureInfo.GetCultureInfo(""x"")) <> ""x""
        b = ""x"" = ""y"".ToLower(System.Globalization.CultureInfo.GetCultureInfo(""x""))
        b = ""x"" <> ""y"".ToLower(System.Globalization.CultureInfo.GetCultureInfo(""x""))
    End Sub
End Module
",
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToXInvariant_NonCaseChanging_TheoryData))]
        public static Task CSharp_ToXInvariantComparisonNonCaseChanging(string @case)
            => VerifyCS.VerifyCodeFixAsync($@"using System;
class C
{{
    void M()
    {{
        _ = [|""y"".To{@case}Invariant() == ""x""|];
        _ = [|""y"".To{@case}Invariant() != ""x""|];
        _ = [|""x"" == ""y"".To{@case}Invariant()|];
        _ = [|""x"" != ""y"".To{@case}Invariant()|];
    }}
}}
",
                @"using System;
class C
{
    void M()
    {
        _ = string.Equals(""y"", ""x"", StringComparison.InvariantCultureIgnoreCase);
        _ = !string.Equals(""y"", ""x"", StringComparison.InvariantCultureIgnoreCase);
        _ = string.Equals(""x"", ""y"", StringComparison.InvariantCultureIgnoreCase);
        _ = !string.Equals(""x"", ""y"", StringComparison.InvariantCultureIgnoreCase);
    }
}
");


        [Theory]
        [MemberData(nameof(ToXInvariant_NonCaseChanging_TheoryData))]
        public static Task Bash_ToXInvariantComparisonNonCaseChanging(string @case)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|""y"".To{@case}Invariant() = ""x""|]
        b = [|""y"".To{@case}Invariant() <> ""x""|]
        b = [|""x"" = ""y"".To{@case}Invariant()|]
        b = [|""x"" <> ""y"".To{@case}Invariant()|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = String.Equals(""y"", ""x"", StringComparison.InvariantCultureIgnoreCase)
        b = Not String.Equals(""y"", ""x"", StringComparison.InvariantCultureIgnoreCase)
        b = String.Equals(""x"", ""y"", StringComparison.InvariantCultureIgnoreCase)
        b = Not String.Equals(""x"", ""y"", StringComparison.InvariantCultureIgnoreCase)
    End Sub
End Module
",
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(MixedEqualsComparison_TheoryData))]
        public static Task CSharp_MixedEqualsComparisonTests(string left, string leftInvocation, string right, string rightInvocation, string expectedStringComparison)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|{left}.{leftInvocation} == {right}.{rightInvocation}|];
        _ = [|{left}.{leftInvocation} != {right}.{rightInvocation}|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = string.Equals({left}, {right}, StringComparison.{expectedStringComparison});
        _ = !string.Equals({left}, {right}, StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(MixedEqualsComparison_TheoryData))]
        public static Task Basic_MixedEqualsComparisonTests(string left, string leftInvocation, string right, string rightInvocation, string expectedStringComparison)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|{left}.{leftInvocation} = {right}.{rightInvocation}|]
        b = [|{left}.{leftInvocation} <> {right}.{rightInvocation}|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = String.Equals({left}, {right}, StringComparison.{expectedStringComparison})
        b = Not String.Equals({left}, {right}, StringComparison.{expectedStringComparison})
    End Sub
End Module
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(MixedEqualsComparison_Diagnostic_NoFix_TheoryData))]
        public static Task CSharp_MixedEqualsComparison_Diagnostic_NoFix(string left, string leftInvocation, string right, string rightInvocation)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|{left}.{leftInvocation} == {right}.{rightInvocation}|];
        _ = [|{left}.{leftInvocation} != {right}.{rightInvocation}|];
    }}
}}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M()
    {{
        _ = [|{left}.{leftInvocation} == {right}.{rightInvocation}|];
        _ = [|{left}.{leftInvocation} != {right}.{rightInvocation}|];
    }}
}}
",
                    },
                },
            }.RunAsync();


        [Theory]
        [MemberData(nameof(MixedEqualsComparison_Diagnostic_NoFix_TheoryData))]
        public static Task Basic_MixedEqualsComparison_Diagnostic_NoFix(string left, string leftInvocation, string right, string rightInvocation)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|{left}.{leftInvocation} = {right}.{rightInvocation}|]
        b = [|{left}.{leftInvocation} <> {right}.{rightInvocation}|]
    End Sub
End Module
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M()
        Dim b As Boolean
        b = [|{left}.{leftInvocation} = {right}.{rightInvocation}|]
        b = [|{left}.{leftInvocation} <> {right}.{rightInvocation}|]
    End Sub
End Module
",
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(Switch_TheoryData))]
        public static Task CSharp_SwitchTests(string invocation)
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"using System;
class C
{{
    void M(string key)
    {{
        switch ([|key.{invocation}|])
        {{
        }}
    }}
}}
",
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(Switch_TheoryData))]
        public static Task Basic_SwitchTests(string invocation)
            => new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        $@"Imports System
Module C
    Sub M(key as String)
        Dim b As Boolean
        Select Case [|key.{invocation}|]
        End Select
    End Sub
End Module
",
                    },
                },
            }.RunAsync();
    }
}