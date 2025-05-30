﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlTriggerConstants;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    internal static class MySqlBindingUtilities
    {
        /// <summary>
        /// Builds a connection using the connection string attached to the app setting with name ConnectionStringSetting
        /// </summary>
        /// <param name="connectionStringSetting">The name of the app setting that stores the MySQL connection string</param>
        /// <param name="configuration">Used to obtain the value of the app setting</param>
        /// <exception cref="ArgumentException">
        /// Thrown if ConnectionStringSetting is empty or null
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if configuration is null
        /// </exception>
        /// <returns>The built connection </returns>
        public static MySqlConnection BuildConnection(string connectionStringSetting, IConfiguration configuration)
        {
            return new MySqlConnection(GetConnectionString(connectionStringSetting, configuration));
        }

        public static string GetConnectionString(string connectionStringSetting, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(connectionStringSetting))
            {
                throw new ArgumentException("Must specify ConnectionStringSetting, which should refer to the name of an app setting that " +
                    "contains a MySQL connection string");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            string connectionString = configuration.GetConnectionStringOrSetting(connectionStringSetting);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(connectionString == null ? $"ConnectionStringSetting '{connectionStringSetting}' is missing in your function app settings, please add the setting with a valid MySQL connection string." :
                $"ConnectionStringSetting '{connectionStringSetting}' is empty in your function app settings, please update the setting with a valid MySQL connection string.");
            }
            return connectionString;
        }

        public static string GetWebSiteName(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            string websitename = configuration.GetConnectionStringOrSetting(MySqlBindingConstants.WEBSITENAME);
            // We require a WEBSITE_SITE_NAME for avoiding duplicates if users use the same function name accross apps.
            if (string.IsNullOrEmpty(websitename))
            {
                throw new ArgumentException($"WEBSITE_SITE_NAME cannot be null or empty in your function app settings, please update the setting with a string value.");
            }
            return websitename;
        }

        /// <summary>
        /// Verifies that the table we are trying to initialize trigger on is supported
        /// </summary>
        /// <exception cref="InvalidOperationException">Throw if an error occurs while checking the column_name 'UpdateAtColumnName' in table does not existed</exception>
        public static async Task VerifyTableForTriggerSupported(MySqlConnection connection, string tableName, ILogger logger, CancellationToken cancellationToken)
        {
            string verifyTableExistQuery = $"SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}'";
            using (var verifyTableExistCommand = new MySqlCommand(verifyTableExistQuery, connection))
            using (MySqlDataReader reader = verifyTableExistCommand.ExecuteReaderWithLogging(logger))
            {
                if (!await reader.ReadAsync(cancellationToken))
                {
                    throw new InvalidOperationException($"Could not find the specified table in the database.");
                }
            }

            string verifyTableSupportedQuery = $"SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{UpdateAtColumnName}'";
            using (var verifyTableSupportedCommand = new MySqlCommand(verifyTableSupportedQuery, connection))
            using (MySqlDataReader reader = verifyTableSupportedCommand.ExecuteReaderWithLogging(logger))
            {
                if (!await reader.ReadAsync(cancellationToken))
                {
                    throw new InvalidOperationException($"The specified table does not have the column named '{UpdateAtColumnName}', hence trigger binding cannot be created on this table.");
                }
            }
        }

        /// <summary>
        /// Parses the parameter string into a list of parameters, where each parameter is separated by "," and has the form
        /// "@param1=param2". "@param1" is the parameter name to be used in the query or stored procedure, and param1 is the
        /// parameter value. Parameter name and parameter value are separated by "=". Parameter names/values cannot contain ',' or '='.
        /// A valid parameter string would be "@param1=param1,@param2=param2". Attaches each parsed parameter to command.
        /// If the value of a parameter should be null, use "null", as in @param1=null,@param2=param2".
        /// If the value of a parameter should be an empty string, do not add anything after the equals sign and before the comma,
        /// as in "@param1=,@param2=param2"
        /// </summary>
        /// <param name="parameters">The parameter string to be parsed</param>
        /// <param name="command">The MySqlCommand to which the parsed parameters will be added to</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if command is null
        /// </exception>
        public static void ParseParameters(string parameters, MySqlCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            // If parameters is null, user did not specify any parameters in their function so nothing to parse
            if (!string.IsNullOrEmpty(parameters))
            {
                // Because we remove empty entries, we will ignore any commas that appear at the beginning/end of the parameter list,
                // as well as extra commas that appear between parameter pairs.
                // I.e., ",,@param1=param1,,@param2=param2,,," will be parsed just like "@param1=param1,@param2=param2" is.
                string[] paramPairs = parameters.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string pair in paramPairs)
                {
                    // Note that we don't throw away empty entries here, so a parameter pair that looks like "=@param1=param1"
                    // or "@param2=param2=" is considered malformed
                    string[] items = pair.Split('=');
                    if (items.Length != 2)
                    {
                        throw new ArgumentException("Parameters must be separated by \",\" and parameter name and parameter value must be separated by \"=\", " +
                           "i.e. \"@param1=param1,@param2=param2\". To specify a null value, use null, as in \"@param1=null,@param2=param2\"." +
                           "To specify an empty string as a value, simply do not add anything after the equals sign, as in \"@param1=,@param2=param2\".");
                    }
                    if (!items[0].StartsWith("@", StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new ArgumentException("Parameter name must start with \"@\", i.e. \"@param1=param1,@param2=param2\"");
                    }


                    if (items[1].Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        command.Parameters.Add(new MySqlParameter(items[0], DBNull.Value));
                    }
                    else
                    {
                        command.Parameters.Add(new MySqlParameter(items[0], items[1]));
                    }
                }
            }
        }

        /// <summary>
        /// Builds a MySqlCommand using the query/stored procedure and parameters specified in attribute.
        /// </summary>
        /// <param name="attribute">The MySqlAttribute with the parameter, command type, and command text</param>
        /// <param name="connection">The connection to attach to the MySqlCommand</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the CommandType specified in attribute is neither StoredProcedure nor Text. We only support
        /// commands that refer to the name of a StoredProcedure (the StoredProcedure CommandType) or are themselves
        /// raw queries (the Text CommandType).
        /// </exception>
        /// <returns>The built MySqlCommand</returns>
        public static MySqlCommand BuildCommand(MySqlAttribute attribute, MySqlConnection connection)
        {
            var command = new MySqlCommand
            {
                Connection = connection,
                CommandText = attribute.CommandText
            };
            if (attribute.CommandType == CommandType.StoredProcedure)
            {
                command.CommandType = CommandType.StoredProcedure;
            }
            else if (attribute.CommandType != CommandType.Text)
            {
                throw new ArgumentException("The type of the MySQL attribute for an input binding must be either CommandType.Text for a direct MySQL query, or CommandType.StoredProcedure for a stored procedure.");
            }
            ParseParameters(attribute.Parameters, command);
            return command;
        }

        /// <summary>
        /// Returns a dictionary where each key is a column name and each value is the MySQL row's value for that column
        /// </summary>
        /// <param name="reader">Used to determine the columns of the table as well as the next MySQL row to process</param>
        /// <returns>The built dictionary</returns>
        public static IReadOnlyDictionary<string, object> BuildDictionaryFromMySqlRow(MySqlDataReader reader)
        {
            return Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, i => reader.GetValue(i));
        }

        /// <summary>
        /// Add acute_symbols(`) around the string
        /// </summary>
        /// <param name="s">The string to acute_symbol(`) quote.</param>
        /// <returns>acute_symbol(`) quoted string.</returns>
        public static string AsAcuteQuotedString(this string s)
        {
            return $"`{s.AsAcuteQuoteEscapedString()}`";
        }

        /// <summary>
        /// Returns the string with any acute quote(`) in it escaped (replaced with ``)
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string AsAcuteQuoteEscapedString(this string s)
        {
            s = s.Replace("`", "``");
            return s;
        }

        /// <summary>
        /// replace double acute_symbols(``) to single acute_quote(`) in string
        /// </summary>
        /// <param name="s">The string could contain double acute_symbol(``) quote.</param>
        /// <returns> replace double acute_symbol(``) with single acute quoted string.</returns>
        public static string AsDoubleAcuteQuotedReplaceString(this string s)
        {
            return s.Replace("``", "`");
        }

        /// <summary>
        /// Escape any existing quotes and add quotes around the string.
        /// </summary>
        /// <param name="s">The string to quote.</param>
        /// <returns>The escaped and quoted string.</returns>
        public static string AsSingleQuotedString(this string s)
        {
            return $"'{s.AsSingleQuoteEscapedString()}'";
        }

        /// <summary>
        /// Returns the string with any single quotes in it escaped (replaced with '')
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string AsSingleQuoteEscapedString(this string s)
        {
            s = s.Replace("\\", "\\\\");
            s = s.Replace("'", "\\'");
            return s;
        }

        /// <summary>
        /// Opens a connection and handles some specific errors if they occur.
        /// </summary>
        /// <param name="connection">The connection to open</param>
        /// <param name="cancellationToken">The cancellation token to pass to the OpenAsync call</param>
        /// <returns>The task that will be completed when the connection is made</returns>
        /// <exception cref="InvalidOperationException">Thrown if an error occurred that we want to wrap with more information</exception>
        internal static async Task OpenAsyncWithMySqlErrorHandling(this MySqlConnection connection, CancellationToken cancellationToken)
        {
            try
            {
                await connection.OpenAsync(cancellationToken);
            }
            catch (Exception e)
            {
                MySqlException mysqlEx = e is AggregateException a ? a.InnerExceptions.OfType<MySqlException>().First() :
                    e is MySqlException s ? s : null;
                // Error number for:
                //  A connection was successfully established with the server, but then an error occurred during the login process.
                //  The certificate chain was issued by an authority that is not trusted.
                // Add on some more information to help the user figure out how to solve it
                if (mysqlEx?.Number == -2146893019)
                {
                    throw new InvalidOperationException("The default values for encryption on connections have been changed, please review your configuration to ensure you have the correct values for your server. See https://aka.ms/afsqlext-connection for more details.", e);
                }
                throw;
            }
        }

        /// <summary>
        /// Whether the exception is a MySqlClient deadlock exception (Error Number 1205)
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal static bool IsDeadlockException(this Exception e)
        {
            // See https://learn.microsoft.com/sql/relational-databases/errors-events/mssqlserver-1205-database-engine-error
            // Transaction (Process ID %d) was deadlocked on %.*ls resources with another process and has been chosen as the deadlock victim. Rerun the transaction.
            return (e as MySqlException)?.Number == 1205
                || (e.InnerException as MySqlException)?.Number == 1205;
        }

        /// <summary>
        /// Checks whether the connection state is currently Broken or Closed
        /// </summary>
        /// <param name="conn">The connection to check</param>
        /// <returns>True if the connection is broken or closed, false otherwise</returns>
        internal static bool IsBrokenOrClosed(this MySqlConnection conn)
        {
            return conn.State == ConnectionState.Broken || conn.State == ConnectionState.Closed;
        }

        /// <summary>
        /// Attempts to ensure that this connection is open, if it currently is in a broken state
        /// then it will close the connection and re-open it.
        /// </summary>
        /// <param name="conn">The connection</param>
        /// <param name="forceReconnect">Whether to force the connection to be re-established, regardless of its current state</param>
        /// <param name="logger">Logger to log events to</param>
        /// <param name="connectionName">The name of the connection to display in the log messages</param>
        /// <param name="token">Cancellation token to pass to the Open call</param>
        /// <returns>True if the connection is open, either because it was able to be re-established or because it was already open. False if the connection could not be re-established.</returns>
        internal static async Task<bool> TryEnsureConnected(this MySqlConnection conn,
            bool forceReconnect,
            ILogger logger,
            string connectionName,
            CancellationToken token)
        {
            if (forceReconnect || conn.IsBrokenOrClosed())
            {
                logger.LogWarning($"{connectionName} is broken, attempting to reconnect...");
                try
                {
                    // Sometimes the connection state is listed as open even if a fatal exception occurred, see
                    // https://github.com/dotnet/SqlClient/issues/1874 for details. So in that case we want to first
                    // close the connection so we can retry (otherwise it'll throw saying the connection is still open)
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    await conn.OpenAsync(token);
                    logger.LogInformation($"Successfully re-established {connectionName}!");
                    return true;
                }
                catch (Exception e)
                {
                    logger.LogError($"Exception reconnecting {connectionName}. Exception = {e.Message}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calls ExecuteScalarAsync and logs an error if it fails before rethrowing.
        /// </summary>
        /// <param name="cmd">The MySqlCommand being executed</param>
        /// <param name="logger">The logger</param>
        /// <param name="cancellationToken">The cancellation token to pass to the call</param>
        /// <param name="logCommand">Defaults to false and when set logs the command being executed</param>
        /// <returns>The result of the call</returns>
        public static async Task<object> ExecuteScalarAsyncWithLogging(this MySqlCommand cmd, ILogger logger, CancellationToken cancellationToken, bool logCommand = false)
        {
            try
            {
                if (logCommand)
                {
                    logger.LogDebug($"Executing query");
                }
                return await cmd.ExecuteScalarAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError($"Exception executing query. Message={e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Calls ExecuteNonQueryAsync and logs an error if it fails before rethrowing.
        /// </summary>
        /// <param name="cmd">The MySqlCommand being executed</param>
        /// <param name="logger">The logger</param>
        /// <param name="cancellationToken">The cancellation token to pass to the call</param>
        /// <param name="logCommand">Defaults to false and when set logs the command being executed</param>
        /// <returns>The result of the call</returns>
        public static async Task<int> ExecuteNonQueryAsyncWithLogging(this MySqlCommand cmd, ILogger logger, CancellationToken cancellationToken, bool logCommand = false)
        {
            try
            {
                if (logCommand)
                {
                    logger.LogDebug($"Executing query.");
                }
                return await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError($"Exception executing query. Message={e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Calls ExecuteReader and logs an error if it fails before rethrowing.
        /// </summary>
        /// <param name="cmd">The MySqlCommand being executed</param>
        /// <param name="logger">The logger</param>
        /// <param name="logCommand">Defaults to false and when set logs the command being executed</param>
        /// <returns>The result of the call</returns>
        public static MySqlDataReader ExecuteReaderWithLogging(this MySqlCommand cmd, ILogger logger, bool logCommand = false)
        {
            try
            {
                if (logCommand)
                {
                    logger.LogDebug($"Executing query");
                }
                return cmd.ExecuteReader();
            }
            catch (Exception e)
            {
                logger.LogError($"Exception executing query. Message={e.Message}");
                throw;
            }
        }
    }
}
