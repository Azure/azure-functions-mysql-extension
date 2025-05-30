﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Data;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Microsoft.Azure.Functions.Worker.Extensions.MySql
{
    // The class to define MySql Input Attributes
    public sealed class MySqlInputAttribute : InputBindingAttribute
    {
        /// <summary>
        /// Creates an instance of the <see cref="MySqlInputAttribute"/>, which takes a MySql query or stored procedure to run and returns the output to the function.
        /// </summary>
        /// <param name="commandText">Either a MySql query or stored procedure that will be run in the target database.</param>
        /// <param name="connectionStringSetting">The name of the app setting where the MySql connection string is stored</param>
        /// <param name="commandType">Specifies whether <see cref="CommandText"/> refers to a stored procedure or MySql query string. Defaults to <see cref="CommandType.Text"/></param>
        /// <param name="parameters">Optional - Specifies the parameters that will be used to execute the MySql query or stored procedure. See <see cref="Parameters"/> for more details.</param>
        public MySqlInputAttribute(string commandText, string connectionStringSetting, CommandType commandType = CommandType.Text, string parameters = null)
        {
            this.CommandText = commandText ?? throw new ArgumentNullException(nameof(commandText));
            this.ConnectionStringSetting = connectionStringSetting ?? throw new ArgumentNullException(nameof(connectionStringSetting));
            this.CommandType = commandType;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Creates an instance of the <see cref="MySqlInputAttribute"/>, which takes a MySql query or stored procedure to run and returns the output to the function.
        /// </summary>
        /// <param name="commandText">Either a MySql query or stored procedure that will be run in the target database.</param>
        /// <param name="connectionStringSetting">The name of the app setting where the MySql connection string is stored</param>
        public MySqlInputAttribute(string commandText, string connectionStringSetting) : this(commandText, connectionStringSetting, CommandType.Text, null) { }

        /// <summary>
        /// The name of the app setting where the MySql connection string is stored
        /// (see https://dev.mysql.com/doc/dev/connector-net/latest/api/data_api/MySql.Data.MySqlClient.MySqlConnection.html).
        /// The attributes specified in the connection string are listed here
        /// https://dev.mysql.com/doc/dev/connector-net/latest/api/data_api/MySql.Data.MySqlClient.MySqlConnection.html#MySql_Data_MySqlClient_MySqlConnection__ctor_System_String_
        /// </summary>
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// Either a MySql query or stored procedure that will be run in the target database.
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// Specifies whether <see cref="CommandText"/> refers to a stored procedure or MySql query string.
        /// Use <see cref="CommandType.StoredProcedure"/> for the former, <see cref="CommandType.Text"/> for the latter.
        /// Defaults to <see cref="CommandType.Text"/>.
        /// </summary>
        public CommandType CommandType { get; }

        /// <summary>
        /// Specifies the parameters that will be used to execute the MySql query or stored procedure specified in <see cref="CommandText"/>.
        /// Must follow the format "@param1=param1,@param2=param2". For example, if your MySql query looks like
        /// "select * from Products where cost = @Cost and name = @Name", then Parameters must have the form "@Cost=100,@Name={Name}"
        /// If the value of a parameter should be null, use "null", as in @param1=null,@param2=param2".
        /// If the value of a parameter should be an empty string, do not add anything after the equals sign and before the comma,
        /// as in "@param1=,@param2=param2"
        /// Note that neither the parameter name nor the parameter value can have ',' or '='
        /// </summary>
        public string Parameters { get; }
    }
}
