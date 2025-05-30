﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.OutputBindingSamples
{
    public static class AddProductsArray
    {
        [Function(nameof(AddProductsArray))]
        [MySqlOutput("Products", "MySqlConnectionString")]
        public static async Task<Product[]> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addproducts-array")]
            HttpRequestData req)
        {
            // Upsert the products, which will insert them into the Products table if the primary key (ProductId) for that item doesn't exist.
            // If it does then update it to have the new name and cost
            Product[] prod = await req.ReadFromJsonAsync<Product[]>();
            return prod;
        }
    }
}
