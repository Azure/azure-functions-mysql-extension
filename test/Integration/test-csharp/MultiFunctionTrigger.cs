// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{
    /// <summary>
    /// Used to ensure correct functionality with multiple user functions tracking the same table.
    /// </summary>
    public static class MultiFunctionTrigger
    {
        [FunctionName(nameof(MultiFunctionTrigger1))]
        public static void MultiFunctionTrigger1(
            [MySqlTrigger("Products", "MySqlConnectionString")]
            IReadOnlyList<MySqlChange<Product>> products,
            ILogger logger)
        {
            logger.LogInformation("Trigger1 Changes: " + Utils.JsonSerializeObject(products));
        }

        [FunctionName(nameof(MultiFunctionTrigger2))]
        public static void MultiFunctionTrigger2(
            [MySqlTrigger("Products", "MySqlConnectionString")]
            IReadOnlyList<MySqlChange<Product>> products,
            ILogger logger)
        {
            logger.LogInformation("Trigger2 Changes: " + Utils.JsonSerializeObject(products));
        }
    }
}
