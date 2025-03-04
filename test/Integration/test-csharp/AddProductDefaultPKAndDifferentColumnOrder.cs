// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{

    public static class AddProductDefaultPKAndDifferentColumnOrder
    {
        /// <summary>
        /// This shows an example of a MySql Output binding where the target table has a default primary key
        /// of type uniqueidentifier and the column is not included in the output object. The order of the
        /// properties in the POCO is different from the order of the columns in the MySql table. A new row will
        /// be inserted and the uniqueidentifier will be generated by the engine.
        /// </summary>
        /// <param name="req">The original request that triggered the function</param>
        /// <param name="output">The created ProductDefaultPKAndDifferentColumnOrder object</param>
        /// <returns>The CreatedResult containing the new object that was inserted</returns>
        [FunctionName("AddProductDefaultPKAndDifferentColumnOrder")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "addproductdefaultpkanddifferentcolumnorder")] HttpRequest req,
            [MySql("ProductsWithDefaultPK", "MySqlConnectionString")] out ProductDefaultPKAndDifferentColumnOrder output)
        {
            output = new ProductDefaultPKAndDifferentColumnOrder
            {
                Cost = 100,
                Name = "test"
            };
            return new CreatedResult($"/api/addproductdefaultpkanddifferentcolumnorder", output);
        }
    }
}