// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples
{
    public static class GetProductsLimitN
    {
        [FunctionName("GetProductsLimitN")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproductlimitn/{count}")]
            HttpRequest req,
            [MySql("SELECT * FROM Products LIMIT {Count}",
                "MySqlConnectionString",
                parameters: "@Count={count}")]
            IEnumerable<Product> products)
        {
            return new OkObjectResult(products);
        }
    }
}
