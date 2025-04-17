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
    public class ProductsTrigger
    {
        private static readonly Action<ILogger, string, Exception> _loggerMessage = LoggerMessage.Define<string>(LogLevel.Information, eventId: new EventId(0, "INFO"), formatString: "{Message}");

        [Function(nameof(ProductsTrigger))]
        public static void Run(
            [MySqlTrigger("Products", "MySqlConnectionString")]
            IReadOnlyList<MySqlChange<Product>> changes, FunctionContext context)
        {
            // The output is used to inspect the trigger binding parameter in test methods.
            if (changes != null && changes.Count > 0)
            {
                _loggerMessage(context.GetLogger("ProductsTrigger"), "MySQL Changes: " + JsonConvert.SerializeObject(changes), null);
            }
        }
    }
}
