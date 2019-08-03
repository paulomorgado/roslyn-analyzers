// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
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
        internal const string ToLowerCaseChangingMethodName = nameof(string.ToLower);
        internal const string ToUpperCaseChangingMethodName = nameof(string.ToUpper);
        internal const string ToLowerInvariantCaseChangingMethodName = nameof(string.ToLowerInvariant);
        internal const string ToUpperInvariantCaseChangingMethodName = nameof(string.ToUpperInvariant);
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCreateStringsForComparisonTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCreateStringsForComparisonMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DoNotCreateStringsForComparisonDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

#pragma warning disable CA1308 // Normalize strings to uppercase
        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
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
            ToLowerCaseChangingMethodName,
            ToUpperCaseChangingMethodName,
            ToLowerInvariantCaseChangingMethodName,
            ToUpperInvariantCaseChangingMethodName);

        internal static readonly ImmutableArray<string> CultureInfoProperties = ImmutableArray.Create(
            nameof(System.Globalization.CultureInfo.CurrentCulture),
            nameof(System.Globalization.CultureInfo.InvariantCulture));

        /// <summary>
        /// Returns a set of descriptors for the diagnostics that this analyzer is capable of producing.
        /// </summary>
        /// <value>The supported diagnostics.</value>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

                    if (IsEqualityOperation(binaryOperation) &&
                        (hasCaseChangingMethodInvocation(binaryOperation.LeftOperand) ||
                            hasCaseChangingMethodInvocation(binaryOperation.RightOperand)))
                    {
                        operationAnalysisContext.ReportDiagnostic(
                            binaryOperation.Syntax.CreateDiagnostic(
                                Rule));
                    }
                }, OperationKind.BinaryOperator);

                compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
                {
                    var invocationOperation = (IInvocationOperation)operationAnalysisContext.Operation;

                    if (isEqualsMethodInvocation(invocationOperation) &&
                        (isInstanceMethodWithCaseChangingMethodInvocation(invocationOperation) ||
                            isStaticMethodWithCaseChangingMethodInvocation(invocationOperation)))
                    {
                        operationAnalysisContext.ReportDiagnostic(
                            invocationOperation.Syntax.CreateDiagnostic(
                                Rule));
                    }

                }, OperationKind.Invocation);

                bool hasCaseChangingMethodInvocation(IOperation operation)
                    => operation.Type.Equals(stringType) &&
                        operation is IInvocationOperation invocationExpression &&
                        CaseChangingMethodNames.Contains(invocationExpression.TargetMethod.Name, StringComparer.Ordinal) &&
                        (invocationExpression.Arguments.Length == 0 ||
                            isCultureInfoProperty(invocationExpression.Arguments[0]));

                bool isCultureInfoProperty(IArgumentOperation argumentOperation)
                    => argumentOperation.Value is IPropertyReferenceOperation propertyOperation &&
                        CultureInfoProperties.Contains(propertyOperation.Property.Name, StringComparer.Ordinal) &&
                        propertyOperation.Property.ContainingSymbol.Equals(cultureInfoType);

                bool isInstanceMethodWithCaseChangingMethodInvocation(IInvocationOperation invocationOperation)
                    => invocationOperation.Instance is IOperation instance &&
                        (hasCaseChangingMethodInvocation(instance) ||
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

        private static bool IsEqualityOperation(IBinaryOperation binaryOperation)
            => binaryOperation.OperatorKind != BinaryOperatorKind.Equals ||
                binaryOperation.OperatorKind != BinaryOperatorKind.NotEquals;
    }
}
