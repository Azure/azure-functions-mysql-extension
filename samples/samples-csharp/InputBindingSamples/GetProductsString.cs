﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples
{
    public static class GetProductsString
    {
        [FunctionName(nameof(GetProductsString))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproducts-string/{cost}")]
            HttpRequest req,
            [MySql("select * from Products where cost = @Cost",
                "MySqlConnectionString",
                parameters: "@Cost={cost}")]
            string products)
        {
            // Products is a JSON representation of the returned rows. For example, if there are two returned rows,
            // products could look like:
            // [{"ProductId":1,"Name":"Dress","Cost":100},{"ProductId":2,"Name":"Skirt","Cost":100}]
            return new OkObjectResult(products);
        }
    }
}
