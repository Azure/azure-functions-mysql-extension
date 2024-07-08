// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Web;
using System.Collections.Specialized;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.OutputBindingSamples
{
    public static class AddProductParams
    {
        [Function(nameof(AddProductParams))]
        [MySqlOutput("Products", "MySqlConnectionString")]
        public static Product Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "addproduct-params")]
            HttpRequestData req)
        {
            if (req != null)
            {
                NameValueCollection queryStrings = HttpUtility.ParseQueryString(req.Url.Query);
                var product = new Product()
                {
                    Name = queryStrings["name"],
                    ProductId = int.Parse(queryStrings["productId"], null),
                    Cost = int.Parse(queryStrings["cost"], null)
                };
                return product;
            }
            return null;
        }
    }
}
