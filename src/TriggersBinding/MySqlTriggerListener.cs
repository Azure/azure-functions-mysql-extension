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
using Microsoft.Azure.WebJobs.Host.Scale;
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
        private readonly int _maxChangesPerWorker;

        private MySqlTableChangeMonitor<T> _changeMonitor;
        private readonly IScaleMonitor<MySqlTriggerMetrics> _scaleMonitor;
        private readonly ITargetScaler _targetScaler;

        private int _listenerState = ListenerNotStarted;
        private string _bracketedLeasesTableName;
        private ulong _userTableId;

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

            int? configuredMaxChangesPerWorker;
            // TODO: when we move to reading them exclusively from the host options, remove reading from settings.
            configuredMaxChangesPerWorker = configuration.GetValue<int?>(ConfigKey_MySqlTrigger_MaxChangesPerWorker);
            this._maxChangesPerWorker = configuredMaxChangesPerWorker ?? this._mysqlOptions.MaxChangesPerWorker;
            if (this._maxChangesPerWorker <= 0)
            {
                throw new InvalidOperationException($"Invalid value for configuration setting '{ConfigKey_MySqlTrigger_MaxChangesPerWorker}'. Ensure that the value is a positive integer.");
            }

            this._scaleMonitor = new MySqlTriggerScaleMonitor(this._userFunctionId, this._userTable, this._userDefinedLeasesTableName, this._connectionString, this._maxChangesPerWorker, this._logger);
            this._targetScaler = new MySqlTriggerTargetScaler(this._userFunctionId, this._userTable, this._userDefinedLeasesTableName, this._connectionString, this._maxChangesPerWorker, this._logger);
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
                    this._userTableId = await GetUserTableIdAsync(connection, this._userTable, this._logger, CancellationToken.None);
                    IReadOnlyList<(string name, string type)> primaryKeyColumns = GetPrimaryKeyColumnsAsync(connection, this._userTable.FullName, this._logger, cancellationToken);
                    IReadOnlyList<string> userTableColumns = this.GetUserTableColumns(connection, this._userTable, cancellationToken);

                    this._bracketedLeasesTableName = GetBracketedLeasesTableName(this._userDefinedLeasesTableName, this._userFunctionId, this._userTableId);

                    long createdSchemaDurationMs = 0L, createGlobalStateTableDurationMs = 0L, insertGlobalStateTableRowDurationMs = 0L, createLeasesTableDurationMs = 0L;

                    using (MySqlTransaction transaction = connection.BeginTransaction(System.Data.IsolationLevel.RepeatableRead))
                    {
                        createdSchemaDurationMs = await this.CreateSchemaAsync(connection, transaction, cancellationToken);
                        createGlobalStateTableDurationMs = await this.CreateGlobalStateTableAsync(connection, transaction, cancellationToken);
                        insertGlobalStateTableRowDurationMs = await this.InsertGlobalStateTableRowAsync(connection, transaction, this._userTableId, cancellationToken);
                        createLeasesTableDurationMs = await this.CreateLeasesTableAsync(connection, transaction, this._bracketedLeasesTableName, primaryKeyColumns, cancellationToken);

                        transaction.Commit();
                    }

                    this._changeMonitor = new MySqlTableChangeMonitor<T>(
                        this._connectionString,
                        this._userTableId,
                        this._userTable,
                        this._userFunctionId,
                        this._bracketedLeasesTableName,
                        primaryKeyColumns,
                        userTableColumns,
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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            int previousState = Interlocked.CompareExchange(ref this._listenerState, ListenerStopping, ListenerStarted);

            if (previousState == ListenerStarted)
            {
                // Clean a record from GlobalStateTableName
                try
                {
                    long deleteGlobalStateTableRowDurationMs = 0L, dropLeasesTableDurationMs = 0L;
                    using (var connection = new MySqlConnection(this._connectionString))
                    {
                        await connection.OpenAsyncWithMySqlErrorHandling(cancellationToken);
                        using (MySqlTransaction transaction = connection.BeginTransaction())
                        {
                            // Call delete global table row for matching functionId & tableId
                            deleteGlobalStateTableRowDurationMs = this.DeleteGlobalStateTableRowAsync(connection, transaction, this._userFunctionId, this._userTableId, cancellationToken).Result;
                            // Call drop leases table method and wait for the result
                            dropLeasesTableDurationMs = this.DropLeasesTableAsync(connection, transaction, this._bracketedLeasesTableName, cancellationToken).Result;

                            transaction.Commit();
                            this._logger.LogInformation($"Cleaned a record from {GlobalStateTableName}, where Function Id: {this._userFunctionId} and the Table Name: {this._userTable.FullName}.");
                            this._logger.LogInformation($"Dropped the lease table {this._bracketedLeasesTableName}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError($"Failed to clean records of trigger binding Function Id: {this._userFunctionId} and the Table Name: {this._userTable.FullName}.\n Exception: {ex.Message}");
                    throw;
                }

                this._changeMonitor.Dispose();
                this._listenerState = ListenerStopped;

            }

            this._logger.LogInformation($"Listener stopped. Duration(ms): {stopwatch.ElapsedMilliseconds}");
        }

        /// <summary>
        /// Gets the column names of the user table.
        /// </summary>
        private IReadOnlyList<string> GetUserTableColumns(MySqlConnection connection, MySqlObject userTable, CancellationToken cancellationToken)
        {
            const int NameIndex = 0;
            string getUserTableColumnsQuery = $@"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE table_name = {userTable.SingleQuotedName}
                    AND table_schema = {userTable.SingleQuotedSchema}
                    AND column_name <> {UpdateAtColumnName.AsSingleQuotedString()};
            ";

            using (var getUserTableColumnsCommand = new MySqlCommand(getUserTableColumnsQuery, connection))
            using (MySqlDataReader reader = getUserTableColumnsCommand.ExecuteReaderWithLogging(this._logger))
            {
                var userTableColumns = new List<string>();
                var userDefinedTypeColumns = new List<(string name, string type)>();

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string columnName = reader.GetString(NameIndex);
                    userTableColumns.Add(columnName);
                }

                this._logger.LogDebug($"GetUserTableColumns ColumnNames = {string.Join(", ", userTableColumns.Select(col => $"'{col}'"))}.");
                return userTableColumns;
            }
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
                        {GlobalStateTableUserFunctionIDColumnName} char(16) NOT NULL,
                        {GlobalStateTableUserTableIDColumnName} int NOT NULL,
                        {GlobalStateTableLastPolledTimeColumnName} Datetime NOT NULL DEFAULT {MYSQL_FUNC_CURRENTTIME},
                        {GlobalStateTableStartPollingTimeColumnName} Datetime NOT NULL DEFAULT {MYSQL_FUNC_CURRENTTIME},
                        PRIMARY KEY ({GlobalStateTableUserFunctionIDColumnName}, {GlobalStateTableUserTableIDColumnName})
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

            string insertRowGlobalStateTableQuery = $"INSERT IGNORE INTO {GlobalStateTableName} ({GlobalStateTableUserFunctionIDColumnName}, {GlobalStateTableUserTableIDColumnName}) VALUES ('{this._userFunctionId}', {userTableId});";

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
        /// <param name="userFunctionId">The ID of the azure function running on table</param>
        /// <param name="userTableId">The ID of the table being watched</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <returns>The time taken in ms to execute the command</returns>
        private async Task<long> DeleteGlobalStateTableRowAsync(MySqlConnection connection, MySqlTransaction transaction, string userFunctionId, ulong userTableId, CancellationToken cancellationToken)
        {
            string deleteRowGlobalStateTableQuery = $"DELETE FROM {GlobalStateTableName} WHERE {GlobalStateTableUserFunctionIDColumnName} = '{userFunctionId}' AND {GlobalStateTableUserTableIDColumnName} = {userTableId};";

            using (var deleteRowGlobalStateTableCommand = new MySqlCommand(deleteRowGlobalStateTableQuery, connection, transaction))
            {
                var stopwatch = Stopwatch.StartNew();
                await deleteRowGlobalStateTableCommand.ExecuteNonQueryAsyncWithLogging(this._logger, cancellationToken);
                return stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Delete a row for the 'user function and table' inside the global state table, if one does already exist.
        /// </summary>
        /// <param name="connection">The already-opened connection to use for executing the command</param>
        /// <param name="transaction">The transaction wrapping this command</param>
        /// <param name="leasesTableName">The leases Table Name need to clean</param>
        /// <param name="cancellationToken">Cancellation token to pass to the command</param>
        /// <returns>The time taken in ms to execute the command</returns>
        private async Task<long> DropLeasesTableAsync(MySqlConnection connection, MySqlTransaction transaction, string leasesTableName, CancellationToken cancellationToken)
        {
            string dropLeasesTableQuery = $"DROP TABLE {leasesTableName};";

            using (var dropLeasesTableCommand = new MySqlCommand(dropLeasesTableQuery, connection, transaction))
            {
                var stopwatch = Stopwatch.StartNew();
                await dropLeasesTableCommand.ExecuteNonQueryAsyncWithLogging(this._logger, cancellationToken);
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

        public IScaleMonitor GetMonitor()
        {
            return this._scaleMonitor;
        }

        public ITargetScaler GetTargetScaler()
        {
            return this._targetScaler;
        }
    }
}
