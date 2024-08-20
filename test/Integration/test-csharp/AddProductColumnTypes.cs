// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{
    public static class AddProductColumnTypes
    {
        /// <summary>
        /// This function is used to test compatability with converting various data types to their respective
        /// MySQL server types.
        /// </summary>
        [FunctionName(nameof(AddProductColumnTypes))]
        public static IActionResult Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "addproduct-columntypes")] HttpRequest req,
                [MySql("ProductsColumnTypes", "MySqlConnectionString")] out ProductColumnTypes product)
        {
            product = new ProductColumnTypes()
            {
                ProductId = int.Parse(req.Query["productId"]),
                BigIntType = int.MaxValue,
                BitType = 1,
                DecimalType = 1.2345M,
                NumericType = 1.2345M,
                SmallIntType = 0,
                TinyIntType = 1,
                FloatType = 1.2,
                RealType = 1.2f,
                DateType = DateTime.Now,
                DatetimeType = DateTime.Now,
                TimeType = DateTime.Now.TimeOfDay,
                CharType = "test",
                VarcharType = "test",
                NcharType = "test",
                NvarcharType = "test",
            };

            // Items were inserted successfully so return success, an exception would be thrown if there
            // was any issues
            return new OkObjectResult("Success!");
        }
    }
}
