// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.OutputBindingSamples
{
    public static class AddProductWithIdentityColumn
    {
        /// <summary>
        /// This shows an example of a MySql Output binding where the target table has a primary key
        /// which is an identity column. In such a case the primary key is not required to be in
        /// the object used by the binding - it will insert a row with the other values and the
        /// ID will be generated upon insert.
        /// </summary>
        /// <param name="req">The original request that triggered the function</param>
        /// <returns>The product object containing the new object that will be upserted</returns>
        [Function(nameof(AddProductWithIdentityColumn))]
        [MySqlOutput("ProductsWithIdentity", "MySqlConnectionString")]
        public static ProductWithoutId Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "addproductwithidentitycolumn")]
            HttpRequestData req)
        {
            NameValueCollection queryStrings = HttpUtility.ParseQueryString(req.Url.Query);
            var product = new ProductWithoutId
            {
                Name = queryStrings["name"],
                Cost = int.Parse(queryStrings["cost"], null)
            };
            return product;
        }
    }
}
