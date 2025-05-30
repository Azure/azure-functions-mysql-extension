// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.TriggerBindingSamples
{
    public class ProductsTriggerLeasesTableName
    {
        private static readonly Action<ILogger, string, Exception> _loggerMessage = LoggerMessage.Define<string>(LogLevel.Information, eventId: new EventId(0, "INFO"), formatString: "{Message}");

        [Function(nameof(ProductsTriggerLeasesTableName))]
        public static void Run(
            [MySqlTrigger("Products", "MySqlConnectionString", "LeasesTable")]
            IReadOnlyList<MySqlChange<Product>> changes, FunctionContext context)
        {
            if (changes != null && changes.Count > 0)
            {
                _loggerMessage(context.GetLogger("ProductsTriggerLeasesTableName"), "MySQL Changes: " + JsonConvert.SerializeObject(changes), null);
            }
        }
    }
}
