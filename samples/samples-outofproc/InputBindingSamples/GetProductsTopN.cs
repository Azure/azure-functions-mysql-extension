// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples
{
    public static class GetProductsTopN
    {
        [Function("GetProductsTopN")]
        public static IEnumerable<Product> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproductstopn/{count}")]
            HttpRequestData req,
            [MySqlInput("SELECT * FROM Products LIMIT {Count}",
                "MySqlConnectionString",
                parameters: "@Count={count}")]
            IEnumerable<Product> products)
        {
            return products;
        }
    }
}
