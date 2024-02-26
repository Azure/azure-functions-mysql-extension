// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using MySql.Data.MySqlClient;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples
{
    public static class GetProductsMySqlCommand
    {
        [FunctionName(nameof(GetProductsMySqlCommand))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getproducts-mysqlcommand/{cost}")]
            HttpRequest req,
            [MySql("select * from Products where cost = @Cost",
                "MySqlConnectionString",
                parameters: "@Cost={cost}")]
            MySqlCommand command)
        {
            string result = string.Empty;
            using (MySqlConnection connection = command.Connection)
            {
                connection.Open();
                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result += $"ProductId: {reader["ProductId"]},  Name: {reader["Name"]}, Cost: {reader["Cost"]}\n";
                }
            }
            return new OkObjectResult(result);
        }
    }
}
