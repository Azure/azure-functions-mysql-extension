// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;
using Microsoft.Extensions.Logging;


namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{
    public static class ProductsColumnTypesTrigger
    {
        /// <summary>
        /// Simple trigger function used to verify different column types are serialized correctly.
        /// </summary>
        [FunctionName(nameof(ProductsColumnTypesTrigger))]
        public static void Run(
            [MySqlTrigger("ProductsColumnTypes", "MySqlConnectionString")]
            IReadOnlyList<MySqlChange<ProductColumnTypes>> changes,
            ILogger logger)
        {
            logger.LogInformation("MySQL Changes: " + Utils.JsonSerializeObject(changes));
        }
    }
}