// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples
{
    /// <summary>
    /// This shows an example of a MySql Input binding that queries from a MySql View named ProductNames.
    /// </summary>
    public static class GetProductNamesView
    {
        [Function(nameof(GetProductNamesView))]
        public static IEnumerable<ProductName> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproduct-namesview/")]
            HttpRequestData req,
            [MySqlInput("SELECT * FROM ProductNames",
                "MySqlConnectionString")]
            IEnumerable<ProductName> products)
        {
            return products;
        }
    }
}
