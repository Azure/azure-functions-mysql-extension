﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.OutputBindingSamples
{
    public static class AddProductsCollector
    {
        [FunctionName(nameof(AddProductsCollector))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "addproducts-collector")]
            HttpRequest req,
            [MySql("Products", "MySqlConnectionString")] ICollector<Product> products)
        {
            List<Product> newProducts = ProductUtilities.GetNewProducts(5000);
            foreach (Product product in newProducts)
            {
                products.Add(product);
            }
            return new CreatedResult($"/api/addproducts-collector", "done");
        }
    }
}
