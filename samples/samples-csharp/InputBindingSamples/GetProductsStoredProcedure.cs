﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples
{

    public static class GetProductsStoredProcedure
    {
        [FunctionName("GetProductsStoredProcedure")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproducts-storedprocedure/{cost}")]
            HttpRequest req,
            [MySql("SelectProductsCost",
                "MySqlConnectionString",
                commandType: System.Data.CommandType.StoredProcedure,
                parameters: "@Cost={cost}")]
            IEnumerable<Product> products)
        {
            return new OkObjectResult(products);
        }
    }
}