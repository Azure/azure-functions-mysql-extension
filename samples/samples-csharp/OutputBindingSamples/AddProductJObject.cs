// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.OutputBindingSamples
{
    public static class AddProductJObject
    {
        [FunctionName(nameof(AddProductJObject))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addproductjobject")]
            [FromBody] JObject prod,
            [MySql("`Products`", "MySqlConnectionString")] out JObject product)
        {
            product = prod;
            return new CreatedResult($"/api/addproduct", product);
        }
    }
}
