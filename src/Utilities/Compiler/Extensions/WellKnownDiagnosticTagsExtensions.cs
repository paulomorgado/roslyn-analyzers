﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis
{
    internal static class WellKnownDiagnosticTagsExtensions
    {
        public const string Dataflow = nameof(Dataflow);
        public static string[] DataflowAndTelemetry = new string[] { Dataflow, WellKnownDiagnosticTags.Telemetry };
    }
}
