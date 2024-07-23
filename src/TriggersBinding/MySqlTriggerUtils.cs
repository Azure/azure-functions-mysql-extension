// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

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
    }

}
