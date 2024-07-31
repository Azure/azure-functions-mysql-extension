// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{
    public static class AddProductsNoPartialUpsert
    {
        public const int UpsertBatchSize = 100;

        // This output binding should throw an error since the ProductsNameNotNull table does not 
        // allows rows without a Name value. No rows should be upserted to the MySql table.
        [FunctionName("AddProductsNoPartialUpsert")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addproducts-nopartialupsert")]
            HttpRequest req,
            [MySql("ProductsNameNotNull", "MySqlConnectionString")] ICollector<Product> products)
        {
            List<Product> newProducts = ProductUtilities.GetNewProducts(UpsertBatchSize);
            foreach (Product product in newProducts)
            {
                products.Add(product);
            }

            var invalidProduct = new Product
            {
                Name = null,
                ProductId = UpsertBatchSize,
                Cost = 100
            };
            products.Add(invalidProduct);

            return new CreatedResult($"/api/addproducts-nopartialupsert", "done");
        }
    }
}
