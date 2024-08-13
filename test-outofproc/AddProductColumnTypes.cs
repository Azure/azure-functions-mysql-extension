// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Web;
using DotnetIsolatedTests.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;

namespace DotnetIsolatedTests
{
    public static class AddProductColumnTypes
    {
        /// <summary>
        /// This function is used to test compatability with converting various data types to their respective
        /// MySQL server types.
        /// </summary>
        [Function(nameof(AddProductColumnTypes))]
        [MySqlOutput("ProductsColumnTypes", "MySqlConnectionString")]
        public static ProductColumnTypes Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "addproduct-columntypes")] HttpRequestData req)
        {
            NameValueCollection queryStrings = HttpUtility.ParseQueryString(req.Url.Query);
            var product = new ProductColumnTypes()
            {
                ProductId = int.Parse(queryStrings["productId"], null),
                // Integer Types in MySql. reference: https://dev.mysql.com/doc/refman/8.0/en/numeric-types.html
                BigIntType = int.MaxValue,
                BitType = true,
                DecimalType = 1.2345M,
                NumericType = 1.2345M,
                SmallIntType = 0,
                TinyIntType = 1,
                FloatType = 1.2,
                RealType = 1.2f,
                DateType = DateTime.UtcNow,
                DatetimeType = new SqlDateTime(DateTime.UtcNow).Value,
                TimeType = DateTime.UtcNow.TimeOfDay,
                CharType = "test",
                VarcharType = "test",
                NcharType = "test",
                NvarcharType = "test"
            };
            return product;
        }
    }
}
