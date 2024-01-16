// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlBindingConstants;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <typeparam name="T">A user-defined POCO that represents a row of the user's table</typeparam>
    internal class MySqlAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        public MySqlConnection Connection { get; private set; }
        private readonly MySqlAttribute _attribute;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlAsyncEnumerable{T}"/> class.
        /// </summary>
        /// <param name="connection">The MySqlConnection to be used by the enumerator</param>
        /// <param name="attribute">The attribute containing the query, parameters, and query type</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either connection or attribute is null
        /// </exception>
        public MySqlAsyncEnumerable(MySqlConnection connection, MySqlAttribute attribute)
        {
            this.Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this._attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            this.Connection.Open();
        }
        /// <summary>
        /// Returns the enumerator associated with this enumerable. The enumerator will execute the query specified
        /// in attribute and "lazily" grab the MySql rows corresponding to the query result. It will only read a
        /// row into memory if <see cref="MySqlAsyncEnumerator.MoveNextAsync"/> is called
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken is not used in this method</param>
        /// <returns>The enumerator</returns>
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new MySqlAsyncEnumerator(this.Connection, this._attribute);
        }


        private class MySqlAsyncEnumerator : IAsyncEnumerator<T>
        {
            private readonly MySqlConnection _connection;
            private readonly MySqlAttribute _attribute;
            private MySqlDataReader _reader;
            /// <summary>
            /// Initializes a new instance of the <see cref="MySqlAsyncEnumerator"/> class.
            /// </summary>
            /// <param name="connection">The MySqlConnection to be used by the enumerator</param>
            /// <param name="attribute">The attribute containing the query, parameters, and query type</param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if either connection or attribute is null
            /// </exception>
            public MySqlAsyncEnumerator(MySqlConnection connection, MySqlAttribute attribute)
            {
                this._connection = connection ?? throw new ArgumentNullException(nameof(connection));
                this._attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            }

            /// <summary>
            /// Returns the current row of the query result that the enumerator is on. If Current is called before a call
            /// to <see cref="MoveNextAsync"/> is ever made, it will return null. If Current is called after
            /// <see cref="MoveNextAsync"/> has moved through all of the rows returned by the query, it will return
            /// the last row of the query.
            /// </summary>
            public T Current { get; private set; }

            /// <summary>
            /// Closes the MySql connection and resources associated with reading the results of the query
            /// </summary>
            /// <returns></returns>
            public ValueTask DisposeAsync()
            {
                // Doesn't seem like there's an async version of closing the reader/connection
                this._reader?.Close();
                this._connection.Close();
                return new ValueTask(Task.CompletedTask);
            }

            /// <summary>
            /// Moves the enumerator to the next row of the MySql query result
            /// </summary>
            /// <returns>
            /// True if there is another row left in the query to process, or false if this was the last row
            /// </returns>
            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(this.GetNextRow());
            }

            /// <summary>
            /// Attempts to grab the next row of the MySql query result.
            /// </summary>
            /// <returns>
            /// True if there is another row left in the query to process, or false if this was the last row
            /// </returns>
            private bool GetNextRow()
            {
                // check connection state before trying to access the reader
                // if DisposeAsync has already closed it due to the issue described here https://github.com/Azure/azure-functions-sql-extension/issues/350
                if (this._connection.State != System.Data.ConnectionState.Closed)
                {
                    if (this._reader == null)
                    {
                        using (MySqlCommand command = MySqlBindingUtilities.BuildCommand(this._attribute, this._connection))
                        {
                            this._reader = command.ExecuteReader();
                        }
                    }
                    if (this._reader.Read())
                    {
                        this.Current = Utils.JsonDeserializeObject<T>(this.SerializeRow());
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Serializes the reader's current MySql row into JSON
            /// </summary>
            /// <returns>JSON string version of the MySql row</returns>
            private string SerializeRow()
            {
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    DateFormatString = ISO_8061_DATETIME_FORMAT
                };
                return Utils.JsonSerializeObject(MySqlBindingUtilities.BuildDictionaryFromMySqlRow(this._reader), jsonSerializerSettings);
            }
        }
    }
}