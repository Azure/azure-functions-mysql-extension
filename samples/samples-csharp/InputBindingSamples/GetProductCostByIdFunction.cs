﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples
{
    public static class GetProductCostByIdFunction
    {
        [FunctionName(nameof(GetProductCostByIdFunction))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproductcostbyid-function/{prodid}")]
            HttpRequest req,
            [MySql("select GetProductCostById(@ProdId) as Cost",
                "MySqlConnectionString",
                parameters: "@ProdId={prodid}")]
            IEnumerable<ProductCost> productcosts)
        {
            return new OkObjectResult(productcosts);
        }
    }
}
