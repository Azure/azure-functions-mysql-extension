﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.TriggerBindingSamples
{
    public static class ProductsTrigger
    {
        [FunctionName(nameof(ProductsTrigger))]
        public static void Run(
            [MySqlTrigger("Products", "MySqlConnectionString")]
            IReadOnlyList<MySqlChange<Product>> changes,
            ILogger logger)
        {
            // The output is used to inspect the trigger binding parameter in test methods.
            foreach (MySqlChange<Product> change in changes)
            {
                Product product = change.Item;
                logger.LogInformation($"Change operation: {change.Operation}");
                logger.LogInformation($"Product Id: {product.ProductId}, Name: {product.Name}, Cost: {product.Cost}");
            }
        }
    }
}
