// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlBindingUtilities;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlTriggerConstants;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlTriggerUtils;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    internal sealed class MySqlTriggerListener<T> : IListener
    {
        private const int ListenerNotStarted = 0;
        private const int ListenerStarting = 1;
        private const int ListenerStarted = 2;
        private const int ListenerStopping = 3;
        private const int ListenerStopped = 4;
        private readonly MySqlObject _userTable;
        private readonly string _connectionString;
        private readonly string _userDefinedLeasesTableName;
        private readonly string _userFunctionId;
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly MySqlOptions _mysqlOptions;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        private MySqlTableChangeMonitor<T> _changeMonitor;
        private int _listenerState = ListenerNotStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTriggerListener{T}"/> class.
        /// </summary>
        /// <param name="connectionString">MySQL connection string used to connect to user database</param>
        /// <param name="tableName">Name of the user table</param>
        /// <param name="userDefinedLeasesTableName">Optional - Name of the leases table</param>
        /// <param name="userFunctionId">Unique identifier for the user function</param>
        /// <param name="executor">Defines contract for triggering user function</param>
        /// <param name="mysqlOptions"></param>
        /// <param name="logger">Facilitates logging of messages</param>
        /// <param name="configuration">Provides configuration values</param>
        public MySqlTriggerListener(string connectionString, string tableName, string userDefinedLeasesTableName, string userFunctionId, ITriggeredFunctionExecutor executor, MySqlOptions mysqlOptions, ILogger logger, IConfiguration configuration)
        {
            this._connectionString = !string.IsNullOrEmpty(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
            this._userTable = !string.IsNullOrEmpty(tableName) ? new MySqlObject(tableName) : throw new ArgumentNullException(nameof(tableName));
            this._userDefinedLeasesTableName = userDefinedLeasesTableName;
            this._userFunctionId = !string.IsNullOrEmpty(userFunctionId) ? userFunctionId : throw new ArgumentNullException(nameof(userFunctionId));
            this._executor = executor ?? throw new ArgumentNullException(nameof(executor));
            this._mysqlOptions = mysqlOptions ?? throw new ArgumentNullException(nameof(mysqlOptions));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Cancel()
        {
            this.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            // Nothing to dispose.
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            int previousState = Interlocked.CompareExchange(ref this._listenerState, ListenerStarting, ListenerNotStarted);

            switch (previousState)
            {
                case ListenerStarting: throw new InvalidOperationException("The listener is already starting.");
                case ListenerStarted: throw new InvalidOperationException("The listener has already started.");
                default: break;
            }

            try
            {
                using (var connection = new MySqlConnection(this._connectionString))
                {
                    await connection.OpenAsyncWithMySqlErrorHandling(cancellationToken);

                    await VerifyTableForTriggerSupported(connection, this._userTable.FullName, this._logger, cancellationToken);
                    ulong userTableId = await GetUserTableIdAsync(connection, this._userTable, this._logger, CancellationToken.None);
                    IReadOnlyList<(string name, string type)> primaryKeyColumns = GetPrimaryKeyColumnsAsync(connection, this._userTable.FullName, this._logger, cancellationToken);

                    string bracketedLeasesTableName = GetBracketedLeasesTableName(this._userDefinedLeasesTableName, this._userFunctionId, userTableId);

                    long createdSchemaDurationMs = 0L, createGlobalStateTableDurationMs = 0L, insertGlobalStateTableRowDurationMs = 0L, createLeasesTableDurationMs = 0L;

                    using (MySqlTransaction transaction = connection.BeginTransaction(System.Data.IsolationLevel.RepeatableRead))
                    {
                        createdSchemaDurationMs = await this.CreateSchemaAsync(connection, transaction, cancellationToken);
                        createGlobalStateTableDurationMs = await this.CreateGlobalStateTableAsync(connection, transaction, cancellationToken);
                        insertGlobalStateTableRowDurationMs = await this.InsertGlobalStateTableRowAsync(connection, transaction, userTableId, cancellationToken);
                        createLeasesTableDurationMs = await this.CreateLeasesTableAsync(connection, transaction, bracketedLeasesTableName, primaryKeyColumns, cancellationToken);

                        transaction.Commit();
                    }

                    this._changeMonitor = new MySqlTableChangeMonitor<T>(
                        this._connectionString,
                        userTableId,
                        this._userTable,
                        this._userFunctionId,
                        bracketedLeasesTableName,
                        primaryKeyColumns,
                        this._executor,
                        this._mysqlOptions,
                        this._logger,
                        this._configuration);

                    this._listenerState = ListenerStarted;
                    this._logger.LogDebug($"Started MySQL trigger listener for table: '{this._userTable.FullName}', function ID: {this._userFunctionId}");

                    this._logger.LogInformation($"CreatedSchemaDurationMs {createdSchemaDurationMs}. CreateGlobalStateTableDurationMs: {createGlobalStateTableDurationMs}. " +
                        $"InsertGlobalStateTableRowDurationMs: {insertGlobalStateTableRowDurationMs}");
                }
            }
            catch (Exception ex)
            {
                this._listenerState = ListenerNotStarted;
                this._logger.LogError($"Failed to start MySQL trigger listener for table: '{this._userTable.FullName}', function ID: '{this._userFunctionId}'. Exception: {ex}");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            int previousState = Interlocked.CompareExchange(ref this._listenerState, ListenerStopping, ListenerStarted);

            if (previousState == ListenerStarted)
            {
                // Clean a record from GlobalStateTableName
                try
                {
                    long deleteGlobalStateTableRowDurationMs = 0L;
                    using (var connection = new MySqlConnection(this._connectionString))
                    {
                        Task conTask = connection.OpenAsyncWithMySqlErrorHandling(cancellationToken);
                        using (MySqlTransaction transaction = connection.BeginTransaction())
                        {
                            ulong userTableId = GetUserTableIdAsync(connection, this._userTable, this._logger, CancellationToken.None).Result;
                            // Call the async method and wait for the result
                            deleteGlobalStateTableRowDurationMs = this.DeleteGlobalStateTableRowAsync(connection, transaction, userTableId, cancellationToken).Result;
                            transaction.Commit();

                            this._logger.LogInformation($"Cleaned a record from {GlobalStateTableName}, for a Function Id: {this._userFunctionId} and the Table: '{this._userTable.FullName}'.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError($"Failed to clean records for MySQL table: '{this._userTable.FullName}' from {GlobalStateTableName}. Exception: {ex}");
                    throw;
                }

                this._changeMonitor.Dispose();
                this._listenerState = ListenerStopped;

            }

            this._logger.LogInformation($"Listener stopped. Duration(ms): {stopwatch.ElapsedMilliseconds}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates the schema for global state table and leases tables, if it does not already exist.
        /// </summary>
        /// <param name="connection">The already-opened connection to use for executing the command</param>
        /// <param name="transaction">The transaction wrapping this command</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <returns>The time taken in ms to execute the command</returns>
        private async Task<long> CreateSchemaAsync(MySqlConnection connection, MySqlTransaction transaction, CancellationToken cancellationToken)
        {
            string createSchemaQuery = $@"CREATE DATABASE IF NOT EXISTS {SchemaName};";

            using (var createSchemaCommand = new MySqlCommand(createSchemaQuery, connection, transaction))
            {
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    await createSchemaCommand.ExecuteNonQueryAsyncWithLogging(this._logger, cancellationToken);
                }
                catch (Exception ex)
                {
                    // TelemetryInstance.TrackException(TelemetryErrorName.CreateSchema, ex, this._telemetryProps);
                    var mysqlEx = ex as MySqlException;
                    if (mysqlEx?.Number == 1007)        // https://mysqlconnector.net/api/mysqlconnector/mysqlerrorcodetype/
                    {
                        // This generally shouldn't happen since we check for its existence in the statement but occasionally
                        // a race condition can make it so that multiple instances will try and create the schema at once.
                        // In that case we can just ignore the error since all we care about is that the schema exists at all.
                        this._logger.LogWarning($"Failed to create schema '{SchemaName}'. Exception message: {ex.Message} This is informational only, function startup will continue as normal.");
                    }
                    else
                    {
                        this._logger.LogError($"Exception encountered while creating schema for global state table and leases tables. Message: {ex.Message}");
                        throw;
                    }
                }

                return stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Creates the global state table if it does not already exist.
        /// </summary>
        /// <param name="connection">The already-opened connection to use for executing the command</param>
        /// <param name="transaction">The transaction wrapping this command</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <returns>The time taken in ms to execute the command</returns>
        private async Task<long> CreateGlobalStateTableAsync(MySqlConnection connection, MySqlTransaction transaction, CancellationToken cancellationToken)
        {
            string createGlobalStateTableQuery = $@"
                    CREATE TABLE IF NOT EXISTS {GlobalStateTableName} (
                        UserFunctionID char(16) NOT NULL,
                        UserTableID int NOT NULL,
                        LastPolledTime Datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        PRIMARY KEY (UserFunctionID, UserTableID)
                    );
            ";

            using (var createGlobalStateTableCommand = new MySqlCommand(createGlobalStateTableQuery, connection, transaction))
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    await createGlobalStateTableCommand.ExecuteNonQueryAsyncWithLogging(this._logger, cancellationToken);
                }
                catch (Exception ex)
                {
                    var mysqlEx = ex as MySqlException;
                    if (mysqlEx?.Number == 1050)        // ER_TABLE_EXISTS_ERROR
                    {
                        // This generally shouldn't happen since we check for its existence in the statement but occasionally
                        // a race condition can make it so that multiple instances will try and create the schema at once.
                        // In that case we can just ignore the error since all we care about is that the schema exists at all.
                        this._logger.LogWarning($"Failed to create global state table '{GlobalStateTableName}'. Exception message: {ex.Message} This is informational only, function startup will continue as normal.");
                    }
                    else
                    {
                        this._logger.LogError($"Exception encountered while creating Global State table. Message: {ex.Message}");
                        throw;
                    }
                }
                return stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Inserts row for the 'user function and table' inside the global state table, if one does not already exist.
        /// </summary>
        /// <param name="connection">The already-opened connection to use for executing the command</param>
        /// <param name="transaction">The transaction wrapping this command</param>
        /// <param name="userTableId">The ID of the table being watched</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <returns>The time taken in ms to execute the command</returns>
        private async Task<long> InsertGlobalStateTableRowAsync(MySqlConnection connection, MySqlTransaction transaction, ulong userTableId, CancellationToken cancellationToken)
        {

            string insertRowGlobalStateTableQuery = $"INSERT IGNORE INTO {GlobalStateTableName} (UserFunctionID, UserTableID) VALUES ('{this._userFunctionId}', {userTableId})";

            using (var insertRowGlobalStateTableCommand = new MySqlCommand(insertRowGlobalStateTableQuery, connection, transaction))
            {
                var stopwatch = Stopwatch.StartNew();
                await insertRowGlobalStateTableCommand.ExecuteNonQueryAsyncWithLogging(this._logger, cancellationToken);
                return stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Delete a row for the 'user function and table' inside the global state table, if one does already exist.
        /// </summary>
        /// <param name="connection">The already-opened connection to use for executing the command</param>
        /// <param name="transaction">The transaction wrapping this command</param>
        /// <param name="userTableId">The ID of the table being watched</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <returns>The time taken in ms to execute the command</returns>
        private async Task<long> DeleteGlobalStateTableRowAsync(MySqlConnection connection, MySqlTransaction transaction, ulong userTableId, CancellationToken cancellationToken)
        {
            string deleteRowGlobalStateTableQuery = $"DELETE FROM {GlobalStateTableName} WHERE UserFunctionID = '{this._userFunctionId}' AND UserTableID = {userTableId}";

            using (var deleteRowGlobalStateTableCommand = new MySqlCommand(deleteRowGlobalStateTableQuery, connection, transaction))
            {
                var stopwatch = Stopwatch.StartNew();
                await deleteRowGlobalStateTableCommand.ExecuteNonQueryAsyncWithLogging(this._logger, cancellationToken);
                return stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Creates the leases table for the 'user function and table', if one does not already exist.
        /// </summary>
        /// <param name="connection">The already-opened connection to use for executing the command</param>
        /// <param name="transaction">The transaction wrapping this command</param>
        /// <param name="leasesTableName">The name of the leases table to create</param>
        /// <param name="primaryKeyColumns">The primary keys of the user table this leases table is for</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <returns>The time taken in ms to execute the command</returns>
        private async Task<long> CreateLeasesTableAsync(
            MySqlConnection connection,
            MySqlTransaction transaction,
            string leasesTableName,
            IReadOnlyList<(string name, string type)> primaryKeyColumns,
            CancellationToken cancellationToken)
        {
            string primaryKeysWithTypes = string.Join(", ", primaryKeyColumns.Select(col => $"{col.name} {col.type}"));
            string primaryKeys = string.Join(", ", primaryKeyColumns.Select(col => col.name));

            string createLeasesTableQuery = $@"
                    CREATE TABLE IF NOT EXISTS {leasesTableName} (
                        {primaryKeysWithTypes},
                        {LeasesTableChangeVersionColumnName} bigint NOT NULL,
                        {LeasesTableAttemptCountColumnName} int NOT NULL,
                        {LeasesTableLeaseExpirationTimeColumnName} datetime,
                        PRIMARY KEY ({primaryKeys})
                    );
            ";

            using (var createLeasesTableCommand = new MySqlCommand(createLeasesTableQuery, connection, transaction))
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    await createLeasesTableCommand.ExecuteNonQueryAsyncWithLogging(this._logger, cancellationToken);
                }
                catch (Exception ex)
                {
                    this._logger.LogWarning($"Failed to create leases table '{leasesTableName}'. Exception message: {ex.Message}");
                    throw;

                }
                long durationMs = stopwatch.ElapsedMilliseconds;
                return durationMs;
            }
        }
    }
}
