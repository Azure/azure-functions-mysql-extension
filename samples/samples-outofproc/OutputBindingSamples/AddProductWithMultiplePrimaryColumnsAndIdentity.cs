﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker.Http;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.OutputBindingSamples
{
    public class MultiplePrimaryKeyProductWithoutId
    {
        public int ExternalId { get; set; }

        public string Name { get; set; }

        public int Cost { get; set; }
    }

    public static class AddProductWithMultiplePrimaryColumnsAndIdentity
    {
        /// <summary>
        /// This shows an example of a MySql Output binding where the target table has a primary key 
        /// which is comprised of multiple columns, with one of them being an identity column. In 
        /// such a case the identity column is not required to be in the object used by the binding 
        /// - it will insert a row with the other values and the ID will be generated upon insert.
        /// All other primary key columns are required to be in the object.
        /// </summary>
        /// <param name="req">The original request that triggered the function</param>
        /// <returns>The new product object that will be upserted</returns>
        [Function(nameof(AddProductWithMultiplePrimaryColumnsAndIdentity))]
        [MySqlOutput("ProductsWithMultiplePrimaryColumnsAndIdentity", "MySqlConnectionString")]
        public static MultiplePrimaryKeyProductWithoutId Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "addproductwithmultipleprimarycolumnsandidentity")]
            HttpRequestData req)
        {
            NameValueCollection queryStrings = HttpUtility.ParseQueryString(req.Url.Query);
            var product = new MultiplePrimaryKeyProductWithoutId
            {
                ExternalId = int.Parse(queryStrings["externalId"], null),
                Name = queryStrings["name"],
                Cost = int.Parse(queryStrings["cost"], null)
            };
            return product;
        }
    }
}
