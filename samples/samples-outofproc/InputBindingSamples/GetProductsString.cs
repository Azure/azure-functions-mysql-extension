﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker.Http;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples
{
    public static class GetProductsString
    {
        [Function(nameof(GetProductsString))]
        public static string Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproducts-string/{cost}")]
            HttpRequestData req,
            [MySqlInput("select * from Products where cost = @Cost",
                "MySqlConnectionString",
                parameters: "@Cost={cost}")]
            string products)
        {
            // Products is a JSON representation of the returned rows. For example, if there are two returned rows,
            // products could look like:
            // [{"ProductId":1,"Name":"Dress","Cost":100},{"ProductId":2,"Name":"Skirt","Cost":100}]
            return products;
        }
    }
}
