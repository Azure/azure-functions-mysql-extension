// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.TriggerBindingSamples
{
    public static class ProductsTrigger
    {
        [FunctionName(nameof(ProductsTrigger))]
        public static void Run(
            [MySqlTrigger("CARTESIAN", "MySqlConnectionString")]
            IReadOnlyList<MySqlChange<Product>> changes,
            ILogger logger)
        {
            // The output is used to inspect the trigger binding parameter in test methods.
            logger.LogInformation("MySQL Changes: " + JsonConvert.SerializeObject(changes));
        }
    }
}
