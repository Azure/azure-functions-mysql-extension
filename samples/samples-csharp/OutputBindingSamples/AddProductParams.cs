// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.OutputBindingSamples
{
    public static class AddProductParams
    {
        [FunctionName(nameof(AddProductParams))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "addproduct-params")]
            HttpRequest req,
            [MySql("Products", "MySqlConnectionString")] out Product product)
        {
            product = new Product
            {
                Name = req.Query["name"],
                ProductId = int.Parse(req.Query["productId"]),
                Cost = int.Parse(req.Query["cost"])
            };
            return new CreatedResult($"/api/addproduct", product);
        }
    }
}
