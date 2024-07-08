// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Web;
using DotnetIsolatedTests.Common;
using Google.Protobuf.WellKnownTypes;
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
                // Ineger Types in MySql. reference: https://dev.mysql.com/doc/refman/8.0/en/numeric-types.html
                BigInt = long.MaxValue,
                IntType = int.MaxValue,
                MediumInt = 8388607, // medium int in mysql can store 3 bytes, range (-8388608 .. 8388607)
                SmallInt = short.MaxValue,
                TinyInt = sbyte.MaxValue,
                // Fixed-Point Types (Exact Value)
                DecimalType = 1.2345M,
                Numeric = 1.2345M,
                // Floating-Point Types (Approximate Value)
                FloatType = float.MaxValue,
                DoubleType = double.MaxValue,
                // Bit-Value Type
                Bit = true,
                // DateTime types. reference: https://dev.mysql.com/doc/refman/8.0/en/date-and-time-types.html
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Datetime = new SqlDateTime(DateTime.UtcNow).Value,
                TimeStampType = DateTime.UtcNow.ToTimestamp(),
                Time = DateTime.UtcNow.TimeOfDay,
                Year = DateTime.UtcNow.Year,
                // String Data Types. reference: https://dev.mysql.com/doc/refman/8.0/en/string-types.html
                CharType = "test",
                Varchar = "test",
                Binary = new byte[] { 0, 1, 2 },
                VarBinary = new byte[] { 0, 1, 2, 3 },
                Text = "test",
            };
            return product;
        }
    }
}
