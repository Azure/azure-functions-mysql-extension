// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <summary>
    /// Represents the changed row in the user table.
    /// </summary>
    /// <typeparam name="T">POCO class representing the row in the user table</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MySqlChange{T}"/> class.
    /// </remarks>
    /// <param name="operation">Change operation</param>
    /// <param name="item">POCO representing the row in the user table on which the change operation took place</param>
    public sealed class MySqlChange<T>(MySqlChangeOperation operation, T item)
    {

        /// <summary>
        /// Change operation (insert, or update).
        /// </summary>
        public MySqlChangeOperation Operation { get; } = operation;

        /// <summary>
        /// POCO representing the row in the user table on which the change operation took place.
        /// </summary>
        public T Item { get; } = item;
    }

    /// <summary>
    /// Represents the type of change operation in the table row.
    /// </summary>
    public enum MySqlChangeOperation
    {
        // Identify as Update Operation for any insert or update operation on a row in table
        Update
    }
}
