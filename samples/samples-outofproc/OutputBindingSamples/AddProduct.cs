// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.OutputBindingSamples
{
    public static class AddProduct
    {
        [Function(nameof(AddProduct))]
        [MySqlOutput("Products", "MySqlConnectionString")]
        public static async Task<Product> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addproduct")]
            HttpRequestData req)
        {
            Product prod = await req.ReadFromJsonAsync<Product>();
            return prod;
        }
    }
}
