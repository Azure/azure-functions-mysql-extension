// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples
{
    /// <summary>
    /// This shows an example of a MySQL Input binding that queries from a MySQL View named ProductNames.
    /// </summary>
    public static class GetProductNamesView
    {
        [FunctionName(nameof(GetProductNamesView))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproduct-namesview/")]
            HttpRequest req,
            [MySql("SELECT * FROM ProductNames",
                "MySqlConnectionString")]
            IEnumerable<ProductName> products)
        {
            return new OkObjectResult(products);
        }
    }
}
