﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples
{
    public static class GetProductsColumnTypesSerializationAsyncEnumerable
    {
        /// <summary>
        /// This function verifies that serializing an item with various data types
        /// and different languages works when using IAsyncEnumerable.
        /// </summary>
        [FunctionName(nameof(GetProductsColumnTypesSerializationAsyncEnumerable))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproducts-columntypesserializationasyncenumerable")]
            HttpRequest req,
            [MySql("SELECT * FROM ProductsColumnTypes",
                "MySqlConnectionString")]
            IAsyncEnumerable<ProductColumnTypes> products,
            ILogger log)
        {
            // Test different cultures to ensure that serialization/deserialization works correctly for all types.
            // We expect the datetime types to be serialized in UTC format.
            string language = req.Query["culture"];
            if (!string.IsNullOrEmpty(language))
            {
                CultureInfo.CurrentCulture = new CultureInfo(language);
            }

            var productsList = new List<ProductColumnTypes>();
            await foreach (ProductColumnTypes item in products)
            {
                log.LogInformation(JsonConvert.SerializeObject(item));
                productsList.Add(item);
            }
            return new OkObjectResult(productsList);
        }
    }
}
