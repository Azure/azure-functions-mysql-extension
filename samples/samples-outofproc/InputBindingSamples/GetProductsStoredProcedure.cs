// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples
{

    public static class GetProductsStoredProcedure
    {
        [Function(nameof(GetProductsStoredProcedure))]
        public static IEnumerable<Product> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproducts-storedprocedure/{cost}")]
            HttpRequestData req,
            [MySqlInput("SelectProductsCost",
                "MySqlConnectionString",
                System.Data.CommandType.StoredProcedure,
                "@Cost={cost}")]
            IEnumerable<Product> products)
        {
            return products;
        }
    }
}