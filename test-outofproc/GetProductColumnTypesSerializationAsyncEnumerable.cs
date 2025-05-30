﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;
using DotnetIsolatedTests.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker.Http;

namespace DotnetIsolatedTests
{
    public static class GetProductsColumnTypesSerializationAsyncEnumerable
    {
        /// <summary>
        /// This function verifies that serializing an item with various data types
        /// and different languages works when using IAsyncEnumerable.
        /// </summary>
        [Function(nameof(GetProductsColumnTypesSerializationAsyncEnumerable))]
        public static async Task<List<ProductColumnTypes>> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproducts-columntypesserializationasyncenumerable")]
            HttpRequestData req,
            [MySqlInput("SELECT * FROM ProductsColumnTypes",
                "MySqlConnectionString")]
            IAsyncEnumerable<ProductColumnTypes> products)
        {
            // Test different cultures to ensure that serialization/deserialization works correctly for all types.
            // We expect the datetime types to be serialized in UTC format.
            string language = HttpUtility.ParseQueryString(req.Url.Query)["culture"];
            if (!string.IsNullOrEmpty(language))
            {
                CultureInfo.CurrentCulture = new CultureInfo(language);
            }

            var productsList = new List<ProductColumnTypes>();
            await foreach (ProductColumnTypes item in products)
            {
                productsList.Add(item);
            }
            return productsList;
        }
    }
}
