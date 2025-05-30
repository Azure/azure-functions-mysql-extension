﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlBindingUtilities;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlTriggerConstants;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlTriggerUtils;

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
            this._userDefinedLeasesTableName = !string.IsNullOrEmpty(userDefinedLeasesTableName) ? new MySqlObject(userDefinedLeasesTableName).Name : userDefinedLeasesTableName;
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
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.HandleException);
                    await connection.OpenAsyncWithMySqlErrorHandling(cancellationToken);

                    // get table id If exists in database
                    string userTableId = await GetUserTableIdAsync(connection, this._userTable, this._logger, CancellationToken.None);
                    await VerifyTableForTriggerSupported(connection, this._userTable.FullName, this._logger, cancellationToken);

                    IReadOnlyList<(string name, string type)> primaryKeyColumns = GetPrimaryKeyColumnsAsync(connection, this._userTable.AcuteQuotedFullName, this._logger, cancellationToken);
                    IReadOnlyList<string> userTableColumns = this.GetUserTableColumns(connection, this._userTable, cancellationToken);

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
                        userTableColumns,
                        this._executor,
                        this._mysqlOptions,
                        this._logger,
                        this._configuration);

                    this._listenerState = ListenerStarted;
                    this._logger.LogDebug($"Started MySQL trigger listener for the table ID: {userTableId} and function ID: {this._userFunctionId}");

                    this._logger.LogInformation($"CreatedSchemaDurationMs {createdSchemaDurationMs}. CreateGlobalStateTableDurationMs: {createGlobalStateTableDurationMs}. " +
                        $"InsertGlobalStateTableRowDurationMs: {insertGlobalStateTableRowDurationMs}");
                }
            }
            catch (Exception ex)
            {
                this._listenerState = ListenerNotStarted;
                this._logger.LogError($"Failed to start MySQL trigger listener for the specified table', function ID: '{this._userFunctionId}'. Exception: {ex}");
                throw;
            }
        }

        public void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            this._logger.LogError($"Unable to establish connection with the provided connection string. Exception: {e.ExceptionObject}");
            Environment.Exit(0);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            int previousState = Interlocked.CompareExchange(ref this._listenerState, ListenerStopping, ListenerStarted);

            if (previousState == ListenerStarted)
            {
                this._changeMonitor.Dispose();
                this._listenerState = ListenerStopped;
            }

            this._logger.LogInformation($"Listener stopped. Duration(ms): {stopwatch.ElapsedMilliseconds}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the column names of the user table.
        /// </summary>
        private IReadOnlyList<string> GetUserTableColumns(MySqlConnection connection, MySqlObject userTable, CancellationToken cancellationToken)
        {
            const int NameIndex = 0, TypeIndex = 1;
            string getUserTableColumnsQuery = $@"
                    SELECT COLUMN_NAME, DATA_TYPE
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
                    string columnType = reader.GetString(TypeIndex);

                    userTableColumns.Add(columnName);

                    // if column data type in a list of unsupported data types then 
                    if (UnsupportedColumnDataTypes.Contains(columnType))
                    {
                        userDefinedTypeColumns.Add((columnName, columnType));
                    }
                }

                if (userDefinedTypeColumns.Count > 0)
                {
                    string columnNamesAndTypes = string.Join(", ", userDefinedTypeColumns.Select(col => $"'{col.name}' (type: {col.type})"));
                    throw new InvalidOperationException($"Found column(s) with unsupported type(s): {columnNamesAndTypes} in table: '{this._userTable.FullName}'.");
                }

                var conflictingColumnNames = userTableColumns.Intersect(ReservedColumnNames).ToList();

                if (conflictingColumnNames.Count > 0)
                {
                    string columnNames = string.Join(", ", conflictingColumnNames.Select(col => $"'{col}'"));
                    throw new InvalidOperationException($"Found reserved column name(s): {columnNames} in table: '{this._userTable.FullName}'." +
                        " Please rename them to be able to use trigger binding.");
                }

                this._logger.LogDebug($"Fetching Table Columns");
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
                        {GlobalStateTableUserTableIDColumnName} char(64) NOT NULL,
                        {GlobalStateTableLastPolledTimeColumnName} Datetime NOT NULL DEFAULT {MYSQL_FUNC_CURRENTTIME},
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
                    this._logger.LogError($"Exception encountered while creating Global State table. Message: {ex.Message}");
                    throw;
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
        private async Task<long> InsertGlobalStateTableRowAsync(MySqlConnection connection, MySqlTransaction transaction, string userTableId, CancellationToken cancellationToken)
        {
            string insertRowGlobalStateTableQuery = $"INSERT IGNORE INTO {GlobalStateTableName} ({GlobalStateTableUserFunctionIDColumnName}, {GlobalStateTableUserTableIDColumnName})" +
                $" VALUES ('{this._userFunctionId}', '{userTableId}');";

            using (var insertRowGlobalStateTableCommand = new MySqlCommand(insertRowGlobalStateTableQuery, connection, transaction))
            {
                var stopwatch = Stopwatch.StartNew();
                await insertRowGlobalStateTableCommand.ExecuteNonQueryAsyncWithLogging(this._logger, cancellationToken);
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
                        {LeasesTableSyncCompletedTime} datetime,
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
