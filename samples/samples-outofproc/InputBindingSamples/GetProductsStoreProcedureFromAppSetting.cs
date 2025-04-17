// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.InputBindingSamples
{
    /// <summary>
    /// This shows an example of a MySql Input binding that uses a stored procedure 
    /// from an app setting value to query for Products with a specific cost that is also defined as an app setting value.
    /// </summary>
    public static class GetProductsStoredProcedureFromAppSetting
    {
        [Function(nameof(GetProductsStoredProcedureFromAppSetting))]
        public static IEnumerable<Product> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproductsbycost")]
            HttpRequestData req,
            [MySqlInput("%Sp_SelectCost%",
                "MySqlConnectionString",
                System.Data.CommandType.StoredProcedure,
                "@Cost=%ProductCost%")]
            IEnumerable<Product> products)
        {
            return products;
        }
    }
}