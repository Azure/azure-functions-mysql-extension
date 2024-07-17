// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlTriggerUtils;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <summary>
    /// Provider class for unprocessed changes metrics for MySql trigger scaling.
    /// </summary>
    internal class MySqlTriggerMetricsProvider
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly MySqlObject _userTable;
        private readonly string _userFunctionId;

        public MySqlTriggerMetricsProvider(string connectionString, ILogger logger, MySqlObject userTable, string userFunctionId)
        {
            this._connectionString = !string.IsNullOrEmpty(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._userTable = userTable ?? throw new ArgumentNullException(nameof(userTable));
            this._userFunctionId = !string.IsNullOrEmpty(userFunctionId) ? userFunctionId : throw new ArgumentNullException(nameof(userFunctionId));
        }
        public async Task<MySqlTriggerMetrics> GetMetricsAsync()
        {
            return new MySqlTriggerMetrics
            {
                UnprocessedChangeCount = await this.GetUnprocessedChangeCountAsync(),
                Timestamp = DateTime.UtcNow,
            };
        }
        private async Task<long> GetUnprocessedChangeCountAsync()
        {
            long unprocessedChangeCount = 0L;

            try
            {
                using (var connection = new MySqlConnection(this._connectionString))
                {
                    await connection.OpenAsync();

                    ulong userTableId = await GetUserTableIdAsync(connection, this._userTable, this._logger, CancellationToken.None);
                    IReadOnlyList<(string name, string type)> primaryKeyColumns = GetPrimaryKeyColumnsAsync(connection, userTableId, this._logger, this._userTable.FullName, CancellationToken.None);

                    // Use a transaction to automatically release the app lock when we're done executing the query
                    using (MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
                    {
                        try
                        {
                            using (MySqlCommand getUnprocessedChangesCommand = this.BuildGetUnprocessedChangesCommand(connection, transaction))
                            {
                                var commandSw = Stopwatch.StartNew();
                                unprocessedChangeCount = (long)await getUnprocessedChangesCommand.ExecuteScalarAsyncWithLogging(this._logger, CancellationToken.None, true);
                                long getUnprocessedChangesDurationMs = commandSw.ElapsedMilliseconds;
                            }

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception ex2)
                            {
                                this._logger.LogError($"GetUnprocessedChangeCount : Failed to rollback transaction due to exception: {ex2.GetType()}. Exception message: {ex2.Message}");
                            }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Failed to query count of unprocessed changes for table '{this._userTable.FullName}' due to exception: {ex.GetType()}. Exception message: {ex.Message}");
                throw;
            }

            return unprocessedChangeCount;
        }
        private MySqlCommand BuildGetUnprocessedChangesCommand(MySqlConnection connection, MySqlTransaction transaction)
        {
            string getUnprocessedChangesQuery = $@"";

            return new MySqlCommand(getUnprocessedChangesQuery, connection, transaction);
        }
    }
}
