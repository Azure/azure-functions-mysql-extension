﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Data;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <summary>
    /// An input and output binding that can be used to either:
    /// - Establish a connection to a MySQL server database and extract the results of a query run against that database, in the case of an input binding
    /// - Establish a connection to a MySQL server database and insert rows into a given table, in the case of an output binding
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class MySqlAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlAttribute"/> class.
        /// </summary>
        /// <param name="commandText">For an input binding, either a MySQL query or stored procedure that will be run in the database. For an output binding, the table name to upsert the values to.</param>
        /// <param name="connectionStringSetting">The name of the app setting where the MySQL connection string is stored</param>
        /// <param name="commandType">Specifies whether <see cref="CommandText"/> refers to a stored procedure or MySQL query string. Defaults to <see cref="CommandType.Text"/></param>
        /// <param name="parameters">Optional - Specifies the parameters that will be used to execute the MySQL query or stored procedure. See <see cref="Parameters"/> for more details.</param>
        public MySqlAttribute(string commandText, string connectionStringSetting, CommandType commandType = CommandType.Text, string parameters = null)
        {
            this.CommandText = commandText ?? throw new ArgumentNullException(nameof(commandText));
            this.ConnectionStringSetting = connectionStringSetting ?? throw new ArgumentNullException(nameof(connectionStringSetting));
            this.CommandType = commandType;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlAttribute"/> class with default values for the CommandType and Parameters.
        /// </summary>
        /// <param name="commandText">For an input binding, either a MySQL query or stored procedure that will be run in the database. For an output binding, the table name to upsert the values to.</param>
        /// <param name="connectionStringSetting">The name of the app setting where the MySQL connection string is stored</param>
        public MySqlAttribute(string commandText, string connectionStringSetting) : this(commandText, connectionStringSetting, CommandType.Text, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlAttribute"/> class with the default value for the CommandType.
        /// </summary>
        /// <param name="commandText">For an input binding, either a MySQL query or stored procedure that will be run in the database. For an output binding, the table name to upsert the values to.</param>
        /// <param name="connectionStringSetting">The name of the app setting where the MySQL connection string is stored</param>
        /// <param name="parameters">Specifies the parameters that will be used to execute the MySQL query or stored procedure. See <see cref="Parameters"/> for more details.</param>
        public MySqlAttribute(string commandText, string connectionStringSetting, string parameters) : this(commandText, connectionStringSetting, CommandType.Text, parameters) { }

        /// <summary>
        /// The name of the app setting where the MySQL connection string is stored
        /// (see https://dev.mysql.com/doc/dev/connector-net/latest/api/data_api/MySql.Data.MySqlClient.MySqlConnection.html).
        /// The attributes specified in the connection string are listed here
        /// https://dev.mysql.com/doc/dev/connector-net/latest/api/data_api/MySql.Data.MySqlClient.MySqlConnection.html#MySql_Data_MySqlClient_MySqlConnection_ConnectionString
        /// For example, to create a connection to the "TestDB" located at the URL "test.database.windows.net" using a User ID and password,
        /// create a ConnectionStringSetting with a name like MySqlServerAuthentication. The value of the MySqlServerAuthentication app setting
        /// would look like "Data Source=test.database.windows.net;Database=TestDB;User ID={userid};Password={password}".
        /// </summary>
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// For an input binding, either a MySQL query or stored procedure that will be run in the target database.
        /// For an output binding, the table name to upsert the values to.
        /// </summary>
        [AutoResolve]
        public string CommandText { get; }

        /// <summary>
        /// Specifies whether <see cref="CommandText"/> refers to a stored procedure or MySQL query string.
        /// Use <see cref="CommandType.StoredProcedure"/> for the former, <see cref="CommandType.Text"/> for the latter.
        /// Defaults to <see cref="CommandType.Text"/>
        /// </summary>
        public CommandType CommandType { get; }

        /// <summary>
        /// Specifies the parameters that will be used to execute the MySQL query or stored procedure specified in <see cref="CommandText"/>.
        /// Must follow the format "@param1=param1,@param2=param2". For example, if your MySQL query looks like
        /// "select * from Products where cost = @Cost and name = @Name", then Parameters must have the form "@Cost=100,@Name={Name}"
        /// If the value of a parameter should be null, use "null", as in @param1=null,@param2=param2".
        /// If the value of a parameter should be an empty string, do not add anything after the equals sign and before the comma,
        /// as in "@param1=,@param2=param2"
        /// Note that neither the parameter name nor the parameter value can have ',' or '='
        /// </summary>
        [AutoResolve]
        public string Parameters { get; }
    }
}