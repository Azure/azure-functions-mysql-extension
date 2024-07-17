// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.TriggerBindingSamples
{
    public static class ProductsTriggerLeasesTableName
    {
        [FunctionName(nameof(ProductsTriggerLeasesTableName))]
        public static void Run(
            [MySqlTrigger("Products", "MySqlConnectionString", "Leases")]
            IReadOnlyList<MySqlChange<Product>> changes,
            ILogger logger)
        {
            logger.LogInformation("MySQL Changes: " + JsonConvert.SerializeObject(changes));
        }
    }
}
