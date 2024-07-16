﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.TriggersBinding
{
    internal class MySqlTriggerTargetScaler : ITargetScaler
    {
        private readonly MySqlTriggerMetricsProvider _metricsProvider;
        private readonly int _maxChangesPerWorker;

        public MySqlTriggerTargetScaler(string userFunctionId, MySqlObject userTable, string connectionString, int maxChangesPerWorker, ILogger logger)
        {
            this._metricsProvider = new MySqlTriggerMetricsProvider(connectionString, logger, userTable, userFunctionId);
            this.TargetScalerDescriptor = new TargetScalerDescriptor(userFunctionId);
            this._maxChangesPerWorker = maxChangesPerWorker;
        }

        public TargetScalerDescriptor TargetScalerDescriptor { get; }

        public async Task<TargetScalerResult> GetScaleResultAsync(TargetScalerContext context)
        {
            MySqlTriggerMetrics metrics = await this._metricsProvider.GetMetricsAsync();

            // Instance concurrency value is set by the functions host when dynamic concurrency is enabled. See https://learn.microsoft.com/en-us/azure/azure-functions/functions-concurrency for more details.
            int concurrency = context.InstanceConcurrency ?? this._maxChangesPerWorker;

            return GetScaleResultInternal(concurrency, metrics.UnprocessedChangeCount);
        }

        internal static TargetScalerResult GetScaleResultInternal(int concurrency, long unprocessedChangeCount)
        {
            if (concurrency < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(concurrency), $"Unexpected concurrency='{concurrency}' - the value must be > 0.");
            }

            int targetWorkerCount = (int)Math.Ceiling(unprocessedChangeCount / (decimal)concurrency);

            return new TargetScalerResult
            {
                TargetWorkerCount = targetWorkerCount
            };
        }
    }
}
