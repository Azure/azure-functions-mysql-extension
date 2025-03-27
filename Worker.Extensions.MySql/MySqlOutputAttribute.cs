// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Microsoft.Azure.Functions.Worker.Extensions.MySql
{
    // The class to define MySql Output Attributes
    /// <summary>
    /// Creates an instance of the <see cref="MySqlOutputAttribute"/>, which takes a list of rows and upserts them into the target table.
    /// </summary>
    /// <param name="commandText">The table name to upsert the values to.</param>
    /// <param name="connectionStringSetting">The name of the app setting where the MySql connection string is stored</param>
    public class MySqlOutputAttribute(string commandText, string connectionStringSetting) : OutputBindingAttribute
    {

        /// <summary>
        /// The name of the app setting where the MySql connection string is stored
        /// (see https://dev.mysql.com/doc/dev/connector-net/latest/api/data_api/MySql.Data.MySqlClient.MySqlConnection.html).
        /// The attributes specified in the connection string are listed here
        /// https://dev.mysql.com/doc/dev/connector-net/latest/api/data_api/MySql.Data.MySqlClient.MySqlConnection.html#MySql_Data_MySqlClient_MySqlConnection__ctor_System_String_
        /// </summary>
        public string ConnectionStringSetting { get; } = connectionStringSetting ?? throw new ArgumentNullException(nameof(connectionStringSetting));

        /// <summary>
        /// The table name to upsert the values to.
        /// </summary>
        public string CommandText { get; } = commandText ?? throw new ArgumentNullException(nameof(commandText));
    }
}
