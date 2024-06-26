﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// These suppression rules override parameters required by functions binding that cannot be converted to discard variables per issue: https://github.com/Azure/azure-functions-dotnet-worker/issues/323
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples.GetProducts.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product})~System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product}")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples.GetProductNamesView.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.ProductName})~System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.ProductName}")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples.GetProductsAsyncEnumerable.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.Collections.Generic.IAsyncEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product})~System.Threading.Tasks.Task{System.Collections.Generic.List{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product}")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples.GetProductsNameEmpty.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product})~System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product}")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples.GetProductsNameNull.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product})~System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product}")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples.GetProductsStoredProcedure.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product})~System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product}")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples.GetProductsStoredProcedureFromAppSetting.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product})~System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product}")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples.GetProductsString.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.String)~System.String")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples.GetProductsLimitN.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product})~System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product}")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.OutputBindingSamples.AddProductsWithIdentityColumnArray.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData)~Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.ProductWithoutId[]")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.OutputBindingSamples.TimerTriggerProducts.Run(Microsoft.Azure.Functions.Worker.TimerInfo,Microsoft.Azure.Functions.Worker.FunctionContext})~System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product}")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unused parameter is required by functions binding", Scope = "member", Target = "~M:Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.MultipleBindingsSamples.GetAndAddProducts.Run(Microsoft.Azure.Functions.Worker.Http.HttpRequestData,System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product})~System.Collections.Generic.IEnumerable{Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common.Product}")]
