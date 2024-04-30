// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using MySql.Data.MySqlClient;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples
{
    public static class GetProductCostByIdFunctionCommand
    {
        [FunctionName(nameof(GetProductCostByIdFunctionCommand))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproductcostbyid-function-command/{prodid}")]
            HttpRequest req,
            [MySql("select GetProductCostById(@ProdId) as Cost",
                "MySqlConnectionString",
                parameters: "@ProdId={prodid}")]
            MySqlCommand command)
        {
            string result = string.Empty;
            using (MySqlConnection connection = command.Connection)
            {
                connection.Open();

                //add return parameters in command
                var returnparam = new MySqlParameter("Cost", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnparam);

                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result += $"Cost: {reader["Cost"]}\n";
                }
            }
            return new OkObjectResult(result);
        }
    }
}
