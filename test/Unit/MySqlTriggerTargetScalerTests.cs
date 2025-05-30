// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Host.Scale;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Unit
{
    public class MySqlTriggerTargetScalerTests
    {
        /// <summary>
        /// Verifies that the scale result returns the expected target worker count.
        /// </summary>
        [Theory]
        [InlineData(6000, null, 6)]
        [InlineData(4500, null, 5)]
        [InlineData(1080, 100, 11)]
        [InlineData(100, null, 1)]
        public void MySqlTriggerTargetScaler_Returns_Expected(int unprocessedChangeCount, int? concurrency, int expected)
        {
            TargetScalerResult result = MySqlTriggerTargetScaler.GetScaleResultInternal(concurrency ?? MySqlOptions.DefaultMaxChangesPerWorker, unprocessedChangeCount);

            Assert.Equal(result.TargetWorkerCount, expected);
        }
    }
}