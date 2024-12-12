// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlTriggerConstants;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    internal static class MySqlTriggerUtils
    {
        /// <summary>
        /// Returns the encrypted Table ID (using schema and table name) of the user table.
        /// <param name="connection">MySQL connection used to connect to user database</param>
        /// <param name="userTable">MySqlObject user table</param>
        /// <param name="logger">Facilitates logging of messages</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <exception cref="InvalidOperationException">Thrown in case of error when querying the object ID for the user table</exception>
        /// </summary>
        internal static async Task<string> GetUserTableIdAsync(MySqlConnection connection, MySqlObject userTable, ILogger logger, CancellationToken cancellationToken)
        {
            string dbName = userTable.Schema;

            // when schem name is same as schema function
            if (dbName.Equals(MySqlObject.SCHEMA_NAME_FUNCTION, StringComparison.Ordinal))
            {
                var getDbCommand = new MySqlCommand($"SELECT {MySqlObject.SCHEMA_NAME_FUNCTION}", connection);

                using (MySqlDataReader reader = getDbCommand.ExecuteReaderWithLogging(logger))
                {
                    if (!await reader.ReadAsync(cancellationToken))
                    {
                        throw new InvalidOperationException($"Received empty response when querying for the database version");
                    }
                    object objDbName = reader.GetValue(0);
                    if (objDbName is DBNull)
                    {
                        throw new InvalidOperationException($"Could not find database.");
                    }

                    dbName = (string)objDbName;
                }
            }

            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dbName + userTable.Name));
                string tableId = new Guid(hash.Take(16).ToArray()).ToString("N");

                return tableId;
            }
        }

        /// <summary>
        /// Gets the names and types of primary key columns of the user table.
        /// </summary>
        /// <param name="connection">MySQL connection used to connect to user database</param>
        /// <param name="userTableName">Name of the user table</param>
        /// <param name="logger">Facilitates logging of messages</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if there are no primary key columns present in the user table or if their names conflict with columns in leases table.
        /// </exception>
        public static IReadOnlyList<(string name, string type)> GetPrimaryKeyColumnsAsync(MySqlConnection connection, string userTableName, ILogger logger, CancellationToken cancellationToken)
        {
            const int NameIndex = 0, TypeIndex = 1;
            string getPrimaryKeyColumnsQuery = $"SHOW COLUMNS FROM " + userTableName + " WHERE `Key` = 'PRI'";

            using (var getPrimaryKeyColumnsCommand = new MySqlCommand(getPrimaryKeyColumnsQuery, connection))
            using (MySqlDataReader reader = getPrimaryKeyColumnsCommand.ExecuteReaderWithLogging(logger))
            {
                var primaryKeyColumns = new List<(string name, string type)>();

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string name = reader.GetString(NameIndex);
                    string type = reader.GetString(TypeIndex);

                    primaryKeyColumns.Add((name, type));
                }

                if (primaryKeyColumns.Count == 0)
                {
                    throw new InvalidOperationException($"Could not find primary key(s) for the given table.");
                }
                return primaryKeyColumns;
            }
        }

        /// <summary>
        /// Returns the formatted leases table name. If userDefinedLeasesTableName is null, the default name Leases_{FunctionId}_{TableId} is used.
        /// </summary>
        /// <param name="userDefinedLeasesTableName">Leases table name defined by the user</param>
        /// <param name="userTableId">MySQL object ID of the user table</param>
        /// <param name="userFunctionId">Unique identifier for the user function</param>
        internal static string GetBracketedLeasesTableName(string userDefinedLeasesTableName, string userFunctionId, string userTableId)
        {
            return string.IsNullOrEmpty(userDefinedLeasesTableName) ? string.Format(CultureInfo.InvariantCulture, LeasesTableNameFormat, $"{userFunctionId}_{userTableId}") :
                string.Format(CultureInfo.InvariantCulture, UserDefinedLeasesTableNameFormat, $"{userDefinedLeasesTableName}");
        }
    }
}
