﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples
{
    public static class GetProductsLimitN
    {
        [Function(nameof(GetProductsLimitN))]
        public static IEnumerable<Product> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproductslimitn/{count}")]
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
