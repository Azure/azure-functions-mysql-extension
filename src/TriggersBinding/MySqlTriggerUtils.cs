// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        /// Returns the object ID of the user table.
        /// </summary>
        /// <param name="connection">MySQL connection used to connect to user database</param>
        /// <param name="userTable">MySqlObject user table</param>
        /// <param name="logger">Facilitates logging of messages</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <exception cref="InvalidOperationException">Thrown in case of error when querying the object ID for the user table</exception>
        internal static async Task<ulong> GetUserTableIdAsync(MySqlConnection connection, MySqlObject userTable, ILogger logger, CancellationToken cancellationToken)
        {
            string getObjectIdQuery = string.Empty;
            string getDbVersion = $"SELECT VERSION()";
            using (var getDbVersionCmd = new MySqlCommand(getDbVersion, connection))
            using (MySqlDataReader reader = getDbVersionCmd.ExecuteReaderWithLogging(logger))
            {
                if (!await reader.ReadAsync(cancellationToken))
                {
                    throw new InvalidOperationException($"Received empty response when querying for the database version");
                }

                object dbVersion = reader.GetValue(0);

                if (dbVersion is DBNull)
                {
                    throw new InvalidOperationException($"Could not find database.");
                }
                logger.LogDebug($"Database version: {dbVersion}");

                getObjectIdQuery = dbVersion.ToString().StartsWith("5.7", StringComparison.InvariantCulture)
                    ? $"SELECT TABLE_ID FROM INFORMATION_SCHEMA.innodb_sys_tables where NAME = CONCAT(DATABASE(), '/', {userTable.QuotedName})"
                    : $"SELECT TABLE_ID FROM INFORMATION_SCHEMA.innodb_tables where NAME = CONCAT(DATABASE(), '/', {userTable.QuotedName})";
            }


            using (var getObjectIdCommand = new MySqlCommand(getObjectIdQuery, connection))
            using (MySqlDataReader reader = getObjectIdCommand.ExecuteReaderWithLogging(logger))
            {
                if (!await reader.ReadAsync(cancellationToken))
                {
                    throw new InvalidOperationException($"Received empty response when querying the Table ID for table: '{userTable.FullName}'.");
                }

                object userTableId = reader.GetValue(0);

                if (userTableId is DBNull)
                {
                    throw new InvalidOperationException($"Could not find table: '{userTable.FullName}'.");
                }
                logger.LogDebug($"GetUserTableId TableId={userTableId}");
                return (ulong)userTableId;
            }
        }

        /// <summary>
        /// Gets the names and types of primary key columns of the user table.
        /// </summary>
        /// <param name="connection">SQL connection used to connect to user database</param>
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
                    throw new InvalidOperationException($"Could not find primary key created in table: '{userTableName}'.");
                }

                logger.LogDebug($"GetPrimaryKeyColumns ColumnNames(types) = {string.Join(", ", primaryKeyColumns.Select(col => $"'{col.name}({col.type})'"))}.");
                return primaryKeyColumns;
            }
        }

        /// <summary>
        /// Returns the formatted leases table name. If userDefinedLeasesTableName is null, the default name Leases_{FunctionId}_{TableId} is used.
        /// </summary>
        /// <param name="userDefinedLeasesTableName">Leases table name defined by the user</param>
        /// <param name="userTableId">SQL object ID of the user table</param>
        /// <param name="userFunctionId">Unique identifier for the user function</param>
        internal static string GetBracketedLeasesTableName(string userDefinedLeasesTableName, string userFunctionId, ulong userTableId)
        {
            return string.IsNullOrEmpty(userDefinedLeasesTableName) ? string.Format(CultureInfo.InvariantCulture, LeasesTableNameFormat, $"{userFunctionId}_{userTableId}") :
                string.Format(CultureInfo.InvariantCulture, UserDefinedLeasesTableNameFormat, $"{userDefinedLeasesTableName.AsBracketQuotedString()}");
        }
    }
}
