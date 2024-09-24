// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{
    public static class ProductsTriggerWithValidation
    {
        /// <summary>
        /// Simple trigger function with additional logic to allow for verifying that the expected number
        /// of changes was received in each batch.
        /// </summary>
        [FunctionName(nameof(ProductsTriggerWithValidation))]
        public static void Run(
            [MySqlTrigger("Products", "MySqlConnectionString")]
            IReadOnlyList<MySqlChange<Product>> changes,
            ILogger logger)
        {
            string expectedMaxBatchSize = Environment.GetEnvironmentVariable("TEST_EXPECTED_MAX_BATCH_SIZE");
            if (!string.IsNullOrEmpty(expectedMaxBatchSize) && int.Parse(expectedMaxBatchSize) != changes.Count)
            {
                throw new Exception($"Invalid max batch size, got {changes.Count} changes but expected {expectedMaxBatchSize}");
            }
            // The output is used to inspect the trigger binding parameter in test methods.
            logger.LogInformation("MySQL Changes: " + Utils.JsonSerializeObject(changes));
        }
    }
}
