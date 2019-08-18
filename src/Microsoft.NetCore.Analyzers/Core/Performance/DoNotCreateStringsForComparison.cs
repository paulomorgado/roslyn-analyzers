// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    /// <summary>
    /// CA1830: Do not create strings for comparison.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCreateStringsForComparisonAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1830";
        internal const string OperationKey = nameof(OperationKey);
        internal const string ShouldNegateKey = nameof(ShouldNegateKey);
        internal const string IsConditionalKey = nameof(IsConditionalKey);
        internal const string OperationEqualsInstanceWithComparison = nameof(OperationEqualsInstanceWithComparison);
        internal const string OperationEqualsInstanceWithoutComparison = nameof(OperationEqualsInstanceWithoutComparison);
        internal const string OperationEqualsStaticWithComparison = nameof(OperationEqualsStaticWithComparison);
        internal const string OperationEqualsStaticWithoutComparison = nameof(OperationEqualsStaticWithoutComparison);
        internal const string OperationBinary = nameof(OperationBinary);
        internal const string OperationSwitch = nameof(OperationSwitch);
        internal const string ToLowerCurrentCultureCaseChangingMethodName = nameof(string.ToLower);
        internal const string ToUpperCurrentCultureCaseChangingMethodName = nameof(string.ToUpper);
        internal const string ToLowerInvariantCultureCaseChangingMethodName = nameof(string.ToLowerInvariant);
        internal const string ToUpperInvariantCultureCaseChangingMethodName = nameof(string.ToUpperInvariant);
        internal const string InvariantCultureIgnoreCase = "InvariantCultureIgnoreCase";
        internal const string CurrentCultureIgnoreCase = nameof(StringComparison.CurrentCultureIgnoreCase);
        internal const string OrdinalIgnoreCase = nameof(StringComparison.OrdinalIgnoreCase);
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCreateStringsForComparisonTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCreateStringsForComparisonMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCreateStringsForComparisonDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

#pragma warning disable CA1308 // Normalize strings to uppercase
        internal static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Performance,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/" + RuleId.ToLowerInvariant(),
            customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
#pragma warning restore CA1308 // Normalize strings to uppercase

        internal static readonly ImmutableArray<string> CaseChangingMethodNames = ImmutableArray.Create(
            ToLowerCurrentCultureCaseChangingMethodName,
            ToUpperCurrentCultureCaseChangingMethodName,
            ToLowerInvariantCultureCaseChangingMethodName,
            ToUpperInvariantCultureCaseChangingMethodName);

        internal static readonly ImmutableArray<string> InvariantCultureCaseChangingMethodNames = ImmutableArray.Create(
            ToLowerInvariantCultureCaseChangingMethodName,
            ToUpperInvariantCultureCaseChangingMethodName);

        internal static readonly ImmutableArray<string> CurrentCultureCaseChangingMethodNames = ImmutableArray.Create(
            ToLowerCurrentCultureCaseChangingMethodName,
            ToUpperCurrentCultureCaseChangingMethodName);

        internal static readonly ImmutableArray<string> StringComparisonModes = ImmutableArray.Create(
            InvariantCultureIgnoreCase,
            CurrentCultureIgnoreCase,
            OrdinalIgnoreCase);

        internal static readonly ImmutableArray<string> CultureInfoProperties = ImmutableArray.Create(
            nameof(System.Globalization.CultureInfo.CurrentCulture),
            nameof(System.Globalization.CultureInfo.InvariantCulture));

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        /// <value>The supported diagnostics.</value>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        /// <summary>
        /// Initializes the specified analysis context.
        /// </summary>
        /// <param name="analysisContext">The analysis context.</param>
        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var stringType = WellKnownTypes.String(compilationStartAnalysisContext.Compilation);
                var cultureInfoType = WellKnownTypes.CultureInfo(compilationStartAnalysisContext.Compilation);

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    var binaryOperation = (IBinaryOperation)operationAnalysisContext.Operation;

                    if ((isEqualityBinaryOperation(binaryOperation) || isInequalityOperationtyBinary(binaryOperation)) &&
                        (hasCaseChangingMethodInvocation(binaryOperation.LeftOperand) ||
                            hasCaseChangingMethodInvocation(binaryOperation.RightOperand)))
                    {
                        var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
                        propertiesBuilder.Add(OperationKey, OperationBinary);
                        if (isInequalityOperationtyBinary(binaryOperation)) propertiesBuilder.Add(ShouldNegateKey, null);
                        var properties = propertiesBuilder.ToImmutable();

                        operationAnalysisContext.ReportDiagnostic(
                            binaryOperation.Syntax.CreateDiagnostic(
                                rule: s_rule,
                                properties: properties));
                    }
                }, OperationKind.BinaryOperator);

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                    {
                        var switchOperation = (ISwitchOperation)operationAnalysisContext.Operation;

                        if (hasCaseChangingMethodInvocation(switchOperation.Value))
                        {
                            var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
                            propertiesBuilder.Add(OperationKey, OperationSwitch);
                            var properties = propertiesBuilder.ToImmutable();

                            operationAnalysisContext.ReportDiagnostic(
                                switchOperation.Value.Syntax.CreateDiagnostic(
                                    rule: s_rule,
                                    properties: properties));
                        }
                    }, OperationKind.Switch);

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                    {
                        var invocationOperation = (IInvocationOperation)operationAnalysisContext.Operation;

                        if (isEqualsMethodInvocation(invocationOperation))
                        {
                            if (isInstanceMethodWithCaseChangingMethodInvocation(invocationOperation))
                            {
                                var isConditional = invocationOperation.Parent is IConditionalAccessOperation;
                                var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
                                propertiesBuilder.Add(
                                    OperationKey,
                                    invocationOperation.Arguments.Length == 1
                                        ? OperationEqualsInstanceWithoutComparison
                                        : OperationEqualsInstanceWithComparison);
                                if (isConditional)
                                {
                                    propertiesBuilder.Add(IsConditionalKey, null);
                                }
                                var properties = propertiesBuilder.ToImmutable();

                                IOperation operation = invocationOperation;
                                while (operation is IInvocationOperation inv && inv.Instance is IConditionalAccessInstanceOperation) operation = operation.Parent.Parent;

                                operationAnalysisContext.ReportDiagnostic(
                                    operation.Syntax.CreateDiagnostic(
                                        rule: s_rule,
                                        properties: properties));
                            }
                            else if (isStaticMethodWithCaseChangingMethodInvocation(invocationOperation))
                            {
                                var propertiesBuilder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
                                propertiesBuilder.Add(
                                    OperationKey,
                                    invocationOperation.Arguments.Length == 2
                                        ? OperationEqualsStaticWithoutComparison
                                        : OperationEqualsStaticWithComparison);
                                var properties = propertiesBuilder.ToImmutable();

                                operationAnalysisContext.ReportDiagnostic(
                                    invocationOperation.Syntax.CreateDiagnostic(
                                        rule: s_rule,
                                        properties: properties));
                            }
                        }

                    }, OperationKind.Invocation);

                bool isEqualityBinaryOperation(IBinaryOperation binaryOperation)
                   => (binaryOperation.OperatorKind == BinaryOperatorKind.Equals ||
                       binaryOperation.OperatorKind == BinaryOperatorKind.ObjectValueEquals) &&
                       binaryOperation.OperatorMethod.ContainingType.Equals(stringType);

                bool isInequalityOperationtyBinary(IBinaryOperation binaryOperation)
                   => (binaryOperation.OperatorKind == BinaryOperatorKind.NotEquals ||
                       binaryOperation.OperatorKind == BinaryOperatorKind.ObjectValueNotEquals) &&
                       binaryOperation.OperatorMethod.ContainingType.Equals(stringType);

                bool hasCaseChangingMethodInvocation(IOperation operation)
                    => operation is IConditionalAccessOperation conditionalAccessOperation && hasCaseChangingMethodInvocation(conditionalAccessOperation.WhenNotNull) ||
                        operation is IInvocationOperation invocationOperation &&
                        CaseChangingMethodNames.Contains(invocationOperation.TargetMethod.Name, StringComparer.Ordinal) &&
                        (invocationOperation.Arguments.Length == 0 ||
                            isCultureInfoProperty(invocationOperation.Arguments[0]));

                bool isCultureInfoProperty(IArgumentOperation argumentOperation)
                    => argumentOperation.Value is IPropertyReferenceOperation propertyOperation &&
                        CultureInfoProperties.Contains(propertyOperation.Property.Name, StringComparer.Ordinal) &&
                        propertyOperation.Property.ContainingSymbol.Equals(cultureInfoType);

                bool isInstanceMethodWithCaseChangingMethodInvocation(IInvocationOperation invocationOperation)
                    => invocationOperation.Instance is IOperation instance &&
                        (hasCaseChangingMethodInvocation(instance is IConditionalAccessInstanceOperation ? ((IConditionalAccessOperation)invocationOperation.Parent).Operation : instance) ||
                            hasCaseChangingMethodInvocation(invocationOperation.Arguments[0].Value));

                bool isStaticMethodWithCaseChangingMethodInvocation(IInvocationOperation invocationOperation)
                    => invocationOperation.Instance is null &&
                        (hasCaseChangingMethodInvocation(invocationOperation.Arguments[0].Value) ||
                            hasCaseChangingMethodInvocation(invocationOperation.Arguments[1].Value));

                bool isEqualsMethodInvocation(IInvocationOperation invocationOperation)
                    => invocationOperation.TargetMethod.Name.Equals(nameof(string.Equals), StringComparison.Ordinal) &&
                        invocationOperation.TargetMethod.ContainingType.Equals(stringType);
            });
        }
    }
}
