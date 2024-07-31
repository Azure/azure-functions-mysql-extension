// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{
    public static class AddProductExtraColumns
    {
        // This output binding should throw an Exception because the ProductExtraColumns object has 
        // two properties that do not exist as columns in the MySql table (ExtraInt and ExtraString).
        [FunctionName("AddProductExtraColumns")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addproduct-extracolumns")]
            HttpRequest req,
            [MySql("Products", "MySqlConnectionString")] out ProductExtraColumns product)
        {
            product = new ProductExtraColumns
            {
                Name = "test",
                ProductId = 1,
                Cost = 100,
                ExtraInt = 1,
                ExtraString = "test"
            };
            return new CreatedResult($"/api/addproduct-extracolumns", product);
        }
    }
}
