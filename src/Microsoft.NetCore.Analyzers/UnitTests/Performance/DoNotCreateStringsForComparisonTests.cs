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

        public static TheoryData< string> Switch_TheoryData = new TheoryData<string>
        {
            { "ToLowerInvariant()" },
            { "ToUpper(System.Globalization.CultureInfo.CurrentCulture)" },
            { "ToUpper(System.Globalization.CultureInfo.InvariantCulture)" },
        };
#pragma warning restore CA2211 // Non-constant fields should not be visible

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task ToXComparison(string expectedStringComparison)
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
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(StringComparison_TheoryData))]
        public static Task ToXInstanceEquals1(string expectedStringComparison)
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
        public static Task ToXStringEquals2(string expectedStringComparison)
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
        public static Task ToXSystemStringEquals2(string expectedStringComparison)
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
        [MemberData(nameof(ToXWithCultureInfo_NonCaseChanging_TheoryData))]
        public static Task ToXWithCultureInfoPropertyComparisonNonCaseChanging(string cultureInfo, string expectedStringComparison)
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

        [Fact]
        public static Task ToXWithCustomCultureInfoomparisonNonCaseChanging()
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

        [Theory]
        [MemberData(nameof(ToXInvariant_NonCaseChanging_TheoryData))]
        public static Task ToXInvariantComparisonNonCaseChanging(string @case)
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
        [InlineData("\"x\"", "ToUpperInvariant()", "\"y\"", "ToLowerInvariant()", "InvariantCultureIgnoreCase")]
        [InlineData("\"x\"", "ToUpper()", "\"y\"", "ToLowerInvariant()", "InvariantCultureIgnoreCase")]
        [InlineData("\"x\"", "ToUpperInvariant()", "\"y\"", "ToLower()", "InvariantCultureIgnoreCase")]
        [InlineData("\"x\"", "ToUpper()", "\"y\"", "ToLower()", "OrdinalIgnoreCase")]
        [InlineData("\"x\"", "ToUpper()", "\"y\"", "ToLower()", "InvariantCultureIgnoreCase")]
        [InlineData("\"x\"", "ToUpper()", "\"y\"", "ToLower()", "CurrentCultureIgnoreCase")]
        public static Task MixedEqualsComparisonTests(string left, string leftInvocation, string right, string rightInvocation, string expectedStringComparison)
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
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [InlineData("\"x\"", "ToUpper(System.Globalization.CultureInfo.CurrentCulture)", "\"y\"", "ToLowerInvariant()")]
        [InlineData("\"x\"", "ToUpperInvariant()", "\"y\"", "ToLower(System.Globalization.CultureInfo.CurrentCulture)")]
        [InlineData("\"x\"", "ToUpper(System.Globalization.CultureInfo.CurrentCulture)", "\"y\"", "ToLower(System.Globalization.CultureInfo.InvariantCulture)")]
        [InlineData("\"x\"", "ToUpper(System.Globalization.CultureInfo.InvariantCulture)", "\"y\"", "ToLower(System.Globalization.CultureInfo.CurrentCulture)")]
        public static Task MixedEqualsComparison_Diagnostic_NoFix(string left, string leftInvocation, string right, string rightInvocation)
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
    }}
}}
",
                    },
                },
            }.RunAsync();

        [Fact]
        public static Task MixedBinaryTests()
            => new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"using System;
class C
{
    void M()
    {
        _ = [|""y"".ToUpperInvariant() == ""x"".ToLower()|];
        _ = [|""y"".ToUpper() == ""x"".ToLowerInvariant()|];
    }
}
",
                    },
                },
                FixedState =
                {
                    Sources =
                    {
                        @"using System;
class C
{
    void M()
    {
        _ = string.Equals(""y"", ""x"", StringComparison.InvariantCultureIgnoreCase);
        _ = string.Equals(""y"", ""x"", StringComparison.InvariantCultureIgnoreCase);
    }
}
",
                    },
                },
            }.RunAsync();

        [Theory]
        [MemberData(nameof(Switch_TheoryData))]
        public static Task SwitchTests(string invocation)
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
    }
}