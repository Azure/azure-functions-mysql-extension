﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.TriggerBindingSamples
{
    public class ProductsTrigger
    {
        private static readonly Action<ILogger, string, Exception> _loggerMessage = LoggerMessage.Define<string>(LogLevel.Information, eventId: new EventId(0, "INFO"), formatString: "{Message}");

        [Function(nameof(ProductsTrigger))]
        public static void Run(
            [MySqlTrigger("Products", "MySqlConnectionString")]
            IReadOnlyList<MySqlChange<Product>> changes, FunctionContext context)
        {
            ILogger logger = context.GetLogger("ProductsTrigger");
            // The output is used to inspect the trigger binding parameter in test methods.
            foreach (MySqlChange<Product> change in changes)
            {
                Product product = change.Item;
                _loggerMessage(logger, $"Change operation: {change.Operation}", null);
                _loggerMessage(logger, $"Product Id: {product.ProductId}, Name: {product.Name}, Cost: {product.Cost}", null);
            }

        }
    }
}
