// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.OutputBindingSamples
{

    public static class AddProductWithDefaultPK
    {
        /// <summary>
        /// This shows an example of a MySql Output binding where the target table has a default primary key
        /// of type uniqueidentifier and the column is not included in the output object. A new row will
        /// be inserted and the uniqueidentifier will be generated by the engine.
        /// </summary>
        /// <param name="req">The original request that triggered the function</param>
        /// <returns>The new product object that will be upserted</returns>
        [Function(nameof(AddProductWithDefaultPK))]
        [MySqlOutput("ProductsWithDefaultPK", "MySqlConnectionString")]
        public static async Task<ProductWithDefaultPK> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addproductwithdefaultpk")]
            HttpRequestData req)
        {
            ProductWithDefaultPK prod = await req.ReadFromJsonAsync<ProductWithDefaultPK>();
            return prod;
        }
    }
}