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
                BigInt = int.MaxValue,
                Bit = true,
                DecimalType = 1.2345M,
                Numeric = 1.2345M,
                SmallInt = 0,
                TinyInt = 1,
                FloatType = 1.2,
                Real = 1.2f,
                Date = DateTime.UtcNow,
                Datetime = new SqlDateTime(DateTime.UtcNow).Value,
                Time = DateTime.UtcNow.TimeOfDay,
                CharType = "test",
                Varchar = "test",
                Nchar = "test",
                Nvarchar = "test"
            };
            return product;
        }
    }
}
