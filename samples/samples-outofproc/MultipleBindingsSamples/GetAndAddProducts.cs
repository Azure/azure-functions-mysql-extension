// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.MultipleBindingsSamples
{
    /// <summary>
    /// This function uses a MySql input binding to get products from the Products table
    /// and upsert those products to the ProductsWithIdentity table.
    /// </summary>
    public static class GetAndAddProducts
    {
        [Function(nameof(GetAndAddProducts))]
        [MySqlOutput("ProductsWithIdentity", "MySqlConnectionString")]
        public static IEnumerable<Product> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getandaddproducts/{cost}")]
            HttpRequestData req,
            [MySqlInput("SELECT * FROM Products where Cost = @Cost",
                "MySqlConnectionString",
                parameters: "@Cost={cost}")]
            IEnumerable<Product> products)
        {
            return products.ToArray();
        }
    }
}
