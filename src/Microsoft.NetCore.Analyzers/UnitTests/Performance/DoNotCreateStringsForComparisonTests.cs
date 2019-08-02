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
        public static TheoryData<string, string> ToX_NonCaseChanging_TheoryData = new TheoryData<string, string>
        {
            { "Lower", "OrdinalIgnoreCase" },
            { "Lower", "InvariantCultureIgnoreCase" },
            { "Lower", "CurrentCultureIgnoreCase" },
            { "Upper", "OrdinalIgnoreCase" },
            { "Upper", "InvariantCultureIgnoreCase" },
            { "Upper", "CurrentCultureIgnoreCase" },
        };

        public static TheoryData<string> ToXInvariant_NonCaseChanging_TheoryData = new TheoryData<string>
        {
            { "Lower" },
            { "Upper" },
        };

        public static TheoryData<string, string, string> ToXWithCultureInfo_NonCaseChanging_TheoryData = new TheoryData<string, string, string>
        {
            { "Lower", nameof(CultureInfo.InvariantCulture), "InvariantCultureIgnoreCase" },
            { "Lower", nameof(CultureInfo.CurrentCulture), "CurrentCultureIgnoreCase" },
            { "Upper", nameof(CultureInfo.InvariantCulture), "InvariantCultureIgnoreCase" },
            { "Upper", nameof(CultureInfo.CurrentCulture), "CurrentCultureIgnoreCase" },
        };
#pragma warning restore CA2211 // Non-constant fields should not be visible

        [Theory]
        [MemberData(nameof(ToX_NonCaseChanging_TheoryData))]
        public static Task ToXEqualsComparisonNonCaseChanging(string @case, string expectedStringComparison)
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
        var s = [|string.Empty.To{@case}() == ""x""|];
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
        var s = string.Equals(string.Empty, ""x"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToX_NonCaseChanging_TheoryData))]
        public static Task ToXNotEqualsComparisonNonCaseChanging(string @case, string expectedStringComparison)
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
        var s = [|""x"" == string.Empty.To{@case}()|];
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
        var s = !string.Equals(string.Empty, ""x"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToX_NonCaseChanging_TheoryData))]
        public static Task NonCaseChangingEqualsComparisonToX(string @case, string expectedStringComparison)
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
        var s = [|string.Empty.To{@case}() == ""x""|];
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
        var s = string.Equals(""x"", string.Empty, StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToX_NonCaseChanging_TheoryData))]
        public static Task NonCaseChangingNotEqualsComparisonToX(string @case, string expectedStringComparison)
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
        var s = [|""x"" != string.Empty.To{@case}()|];
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
        var s = !string.Equals(""x"", string.Empty, StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToXWithCultureInfo_NonCaseChanging_TheoryData))]
        public static Task ToXWithCultureInfoEqualsComparisonNonCaseChanging(string @case, string cultureInfo, string expectedStringComparison)
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
        var s = [|string.Empty.To{@case}(System.Globalization.CultureInfo.{cultureInfo}) == ""x""|];
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
        var s = string.Equals(string.Empty, ""x"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToXWithCultureInfo_NonCaseChanging_TheoryData))]
        public static Task ToXWithCultureInfoNotEqualsComparisonNonCaseChanging(string @case, string cultureInfo, string expectedStringComparison)
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
        var s = [|string.Empty.To{@case}(System.Globalization.CultureInfo.{cultureInfo}) != ""x""|];
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
        var s = !string.Equals(string.Empty, ""x"", StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToXWithCultureInfo_NonCaseChanging_TheoryData))]
        public static Task NonCaseChangingEqualsComparisonToXWithCultureInfo(string @case, string cultureInfo, string expectedStringComparison)
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
        var s = [|""x"" == string.Empty.To{@case}(System.Globalization.CultureInfo.{cultureInfo})|];
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
        var s = string.Equals(""x"", string.Empty, StringComparison.InvariantCultureIgnoreCase);
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToXWithCultureInfo_NonCaseChanging_TheoryData))]
        public static Task NonCaseChangingNotEqualsComparisonToXWithCultureInfo(string @case, string cultureInfo, string expectedStringComparison)
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
        var s = [|""x"" != string.Empty.To{@case}(System.Globalization.CultureInfo.{cultureInfo})|];
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
        var s = !string.Equals(""x"", string.Empty, StringComparison.InvariantCultureIgnoreCase);
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [MemberData(nameof(ToXInvariant_NonCaseChanging_TheoryData))]
        public static Task ToXInvariantEqualsComparisonNonCaseChanging(string @case)
            => VerifyCS.VerifyCodeFixAsync($@"using System;
class C
{{
    void M()
    {{
        var s = [|string.Empty.To{@case}Invariant() == ""x""|];
    }}
}}
",
                @"using System;
class C
{
    void M()
    {
        var s = string.Equals(string.Empty, ""x"", StringComparison.InvariantCultureIgnoreCase);
    }
}
");

        [Theory]
        [MemberData(nameof(ToXInvariant_NonCaseChanging_TheoryData))]
        public static Task NonCaseChangingEqualsComparisonToXInvariant(string @case)
            => VerifyCS.VerifyCodeFixAsync($@"using System;
class C
{{
    void M()
    {{
        var s = [|""x"" == string.Empty.To{@case}Invariant()|];
    }}
}}
",
                @"using System;
class C
{
    void M()
    {
        var s = string.Equals(""x"", string.Empty, StringComparison.InvariantCultureIgnoreCase);
    }
}
");

        [Theory]
        [MemberData(nameof(ToXInvariant_NonCaseChanging_TheoryData))]
        public static Task ToXInvariantNotEqualsComparisonNonCaseChanging(string @case)
            => VerifyCS.VerifyCodeFixAsync($@"using System;
class C
{{
    void M()
    {{
        var s = [|string.Empty.To{@case}Invariant() != ""x""|];
    }}
}}
",
                @"using System;
class C
{
    void M()
    {
        var s = !string.Equals(string.Empty, ""x"", StringComparison.InvariantCultureIgnoreCase);
    }
}
");

        [Theory]
        [MemberData(nameof(ToXInvariant_NonCaseChanging_TheoryData))]
        public static Task NonCaseChangingNotEqualsComparisonToXInvariant(string @case)
            => VerifyCS.VerifyCodeFixAsync($@"using System;
class C
{{
    void M()
    {{
        var s = [|""x"" != string.Empty.To{@case}Invariant()|];
    }}
}}
",
                @"using System;
class C
{
    void M()
    {
        var s = !string.Equals(""x"", string.Empty, StringComparison.InvariantCultureIgnoreCase);
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
        var s = [|{left}.{leftInvocation} == {right}.{rightInvocation}|];
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
        var s = string.Equals({left}, {right}, StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync();

        [Theory]
        [InlineData("\"x\"", "ToUpper()", "\"y\"", "ToLowerInvariant()", "OrdinalIgnoreCase")]
        [InlineData("\"x\"", "ToUpperInvariant()", "\"y\"", "ToLower()", "CurrentCultureIgnoreCase")]
        [InlineData("\"x\"", "ToUpper(System.Globalization.CultureInfo.CurrentCulture)", "\"y\"", "ToLower(System.Globalization.CultureInfo.InvariantCulture)", "OrdinalIgnoreCase")]
        [InlineData("\"x\"", "ToUpper(System.Globalization.CultureInfo.CurrentCulture)", "\"y\"", "ToLower(System.Globalization.CultureInfo.InvariantCulture)", "InvariantCultureIgnoreCase")]
        [InlineData("\"x\"", "ToUpper(System.Globalization.CultureInfo.CurrentCulture)", "\"y\"", "ToLower(System.Globalization.CultureInfo.InvariantCulture)", "CurrentCultureIgnoreCase")]
        public static async Task MixedEqualsComparison_NoDiagnostic_Tests(string left, string leftInvocation, string right, string rightInvocation, string expectedStringComparison)
        {
            await new VerifyCS.Test
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
        var s = [|{left}.{leftInvocation} == {right}.{rightInvocation}|];
    }}
}}
",
                    },
                },
            }.RunAsync();

            await Assert.ThrowsAsync<Xunit.Sdk.TrueException>(() => new VerifyCS.Test
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
        var s = [|{left}.{leftInvocation} == {right}.{rightInvocation}|];
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
        var s = string.Equals({left}, {right}, StringComparison.{expectedStringComparison});
    }}
}}
",
                    },
                },
                CodeFixEquivalenceKey = expectedStringComparison,
            }.RunAsync());
        }
    }
}