// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlBindingConstants;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    internal class MySqlConverters
    {
        internal class MySqlConverter : IConverter<MySqlAttribute, MySqlCommand>
        {
            private readonly IConfiguration _configuration;
            private readonly ILogger _logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="MySqlConverter"/> class.
            /// </summary>
            /// <param name="configuration"></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if the configuration is null
            /// </exception>
            public MySqlConverter(IConfiguration configuration)
            {
                this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            }

            /// <summary>
            /// Creates a MySqlCommand containing a MySQL connection and the MySQL query and parameters specified in attribute.
            /// The user can open the connection in the MySqlCommand and use it to read in the results of the query themselves.
            /// </summary>
            /// <param name="attribute">
            /// Contains the MySQL query and parameters as well as the information necessary to build the MySQL Connection
            /// </param>
            /// <returns>The MySqlCommand</returns>
            public MySqlCommand Convert(MySqlAttribute attribute)
            {
                try
                {
                    return MySqlBindingUtilities.BuildCommand(attribute, MySqlBindingUtilities.BuildConnection(
                                       attribute.ConnectionStringSetting, this._configuration));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception encountered while converting to MySQL command. Message: {ex.Message}");
                    throw;
                }
            }

        }

        /// <typeparam name="T">A user-defined POCO that represents a row of the user's table</typeparam>
        internal class MySqlGenericsConverter<T> : IAsyncConverter<MySqlAttribute, IEnumerable<T>>, IConverter<MySqlAttribute, IAsyncEnumerable<T>>,
            IAsyncConverter<MySqlAttribute, string>, IAsyncConverter<MySqlAttribute, JArray>
        {
            ILogger logger;

            private readonly IConfiguration _configuration;

            /// <summary>
            /// Initializes a new instance of the <see cref="MySqlGenericsConverter{T}"/> class.
            /// </summary>
            /// <param name="configuration"></param>
            /// <param name="logger">ILogger used to log information and warnings</param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if the configuration is null
            /// </exception>
            public MySqlGenericsConverter(IConfiguration configuration, ILogger logger)
            {
                this.logger = logger;
                this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            }

            /// <summary>
            /// Opens a MySqlConnection, reads in the data from the user's database, and returns it as a list of POCOs.
            /// </summary>
            /// <param name="attribute">
            /// Contains the information necessary to establish a MySqlConnection, and the query to be executed on the database
            /// </param>
            /// <param name="cancellationToken">The cancellationToken is not used in this method</param>
            /// <returns>An IEnumerable containing the rows read from the user's database in the form of the user-defined POCO</returns>
            public async Task<IEnumerable<T>> ConvertAsync(MySqlAttribute attribute, CancellationToken cancellationToken)
            {
                try
                {
                    string json = await this.BuildItemFromAttributeAsync(attribute);
                    return Utils.JsonDeserializeObject<IEnumerable<T>>(json);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception encountered in MySqlGenerics Converter - IEnumerable. Message: {ex.Message}");
                    throw;
                }
            }

            /// <summary>
            /// Opens a MySqlConnection, reads in the data from the user's database, and returns it as a JSON-formatted string.
            /// </summary>
            /// <param name="attribute">
            /// Contains the information necessary to establish a MySqlConnection, and the query to be executed on the database
            /// </param>
            /// <param name="cancellationToken">The cancellationToken is not used in this method</param>
            /// <returns>
            /// The JSON string. I.e., if the result has two rows from a table with schema ProductId: int, Name: varchar, Cost: int,
            /// then the returned JSON string could look like
            /// [{"productId":3,"name":"Bottle","cost":90},{"productId":5,"name":"Cup","cost":100}]
            /// </returns>
            async Task<string> IAsyncConverter<MySqlAttribute, string>.ConvertAsync(MySqlAttribute attribute, CancellationToken cancellationToken)
            {
                try
                {
                    return await this.BuildItemFromAttributeAsync(attribute);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception encountered in MySqlGenerics Converter - JSON. Message: {ex.Message}");
                    throw;
                }
            }

            /// <summary>
            /// Extracts the <see cref="MySqlAttribute.ConnectionStringSetting"/> in attribute and uses it to establish a connection
            /// to the MySQL database. (Must be virtual for mocking the method in unit tests)
            /// </summary>
            /// <param name="attribute">
            /// The binding attribute that contains the name of the connection string app setting and query.
            /// </param>
            /// <returns></returns>
            public virtual async Task<string> BuildItemFromAttributeAsync(MySqlAttribute attribute)
            {
                using (MySqlConnection connection = MySqlBindingUtilities.BuildConnection(attribute.ConnectionStringSetting, this._configuration))
                // Ideally, we would like to move away from using MySqlDataAdapter both here and in the
                // MySqlAsyncCollector since it does not support asynchronous operations.
                using (var adapter = new MySqlDataAdapter())
                using (MySqlCommand command = MySqlBindingUtilities.BuildCommand(attribute, connection))
                {
                    adapter.SelectCommand = command;
                    await connection.OpenAsyncWithMySqlErrorHandling(CancellationToken.None);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    logger.LogInformation($"{dataTable.Rows.Count} row(s) queried from database: {connection.Database} using Command: {command.CommandText}");
                    // Serialize any DateTime objects in UTC format
                    var jsonSerializerSettings = new JsonSerializerSettings()
                    {
                        DateFormatString = ISO_8061_DATETIME_FORMAT
                    };
                    return Utils.JsonSerializeObject(dataTable, jsonSerializerSettings);
                }

            }

            IAsyncEnumerable<T> IConverter<MySqlAttribute, IAsyncEnumerable<T>>.Convert(MySqlAttribute attribute)
            {
                try
                {
                    var asyncEnumerable = new MySqlAsyncEnumerable<T>(MySqlBindingUtilities.BuildConnection(attribute.ConnectionStringSetting, this._configuration), attribute);
                    return asyncEnumerable;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception encountered in MySqlGenerics Converter - IAsyncEnumerable. Message: {ex.Message}");
                    throw;
                }
            }

            /// <summary>
            /// Opens a MySqlConnection, reads in the data from the user's database, and returns it as JArray.
            /// </summary>
            /// <param name="attribute">
            /// Contains the information necessary to establish a MySqlConnection, and the query to be executed on the database
            /// </param>
            /// <param name="cancellationToken">The cancellationToken is not used in this method</param>
            /// <returns>JArray containing the rows read from the user's database in the form of the user-defined POCO</returns>
            async Task<JArray> IAsyncConverter<MySqlAttribute, JArray>.ConvertAsync(MySqlAttribute attribute, CancellationToken cancellationToken)
            {
                try
                {
                    string json = await this.BuildItemFromAttributeAsync(attribute);
                    return JArray.Parse(json);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception encountered in MySqlGenerics Converter - JArray. Message: {ex.Message}");
                    throw;
                }
            }

        }
    }
}