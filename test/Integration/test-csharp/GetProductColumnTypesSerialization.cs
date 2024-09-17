// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples
{
    public static class GetProductsColumnTypesSerialization
    {
        /// <summary>
        /// This function verifies that serializing an item with various data types
        /// works as expected when using IEnumerable.
        /// </summary>
        [FunctionName(nameof(GetProductsColumnTypesSerialization))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproducts-columntypesserialization")]
            HttpRequest req,
            [MySql("SELECT * FROM ProductsColumnTypes",
                "MySqlConnectionString")]
            IEnumerable<ProductColumnTypes> products,
            ILogger log)
        {
            foreach (ProductColumnTypes item in products)
            {
                log.LogInformation(JsonConvert.SerializeObject(item));
            }

            return new OkObjectResult(products);
        }
    }
}
