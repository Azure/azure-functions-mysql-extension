﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <summary>
    /// Represents the MySQL trigger parameter binding.
    /// </summary>
    /// <typeparam name="T">POCO class representing the row in the user table</typeparam>

    internal sealed class MySqlTriggerBinding<T> : ITriggerBinding
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _leasesTableName;
        private readonly ParameterInfo _parameter;
        private readonly MySqlOptions _mysqlOptions;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        private static readonly IReadOnlyDictionary<string, Type> _emptyBindingContract = new Dictionary<string, Type>();
        private static readonly IReadOnlyDictionary<string, object> _emptyBindingData = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTriggerBinding{T}"/> class.
        /// </summary>
        /// <param name="connectionString">MySQL connection string used to connect to user database</param>
        /// <param name="tableName">Name of the user table</param>
        /// <param name="leasesTableName">Optional - Name of the leases table</param>
        /// <param name="parameter">Trigger binding parameter information</param>
        /// <param name="mysqlOptions"></param>
        /// <param name="logger">Facilitates logging of messages</param>
        /// <param name="configuration">Provides configuration values</param>
        public MySqlTriggerBinding(string connectionString, string tableName, string leasesTableName, ParameterInfo parameter, IOptions<MySqlOptions> mysqlOptions, ILogger logger, IConfiguration configuration)
        {
            this._connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this._tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            this._leasesTableName = leasesTableName;
            this._parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            this._mysqlOptions = (mysqlOptions ?? throw new ArgumentNullException(nameof(mysqlOptions))).Value;
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Returns the type of trigger value that <see cref="MySqlTriggerBinding{T}" /> binds to.
        /// </summary>
        public Type TriggerValueType => typeof(IReadOnlyList<MySqlChange<T>>);

        public IReadOnlyDictionary<string, Type> BindingDataContract => _emptyBindingContract;

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            IValueProvider valueProvider = new MySqlTriggerValueProvider(this._parameter.ParameterType, value, this._tableName);
            return Task.FromResult<ITriggerData>(new TriggerData(valueProvider, _emptyBindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context), "Missing listener context");

            string userFunctionId = this.GetUserFunctionIdAsync();
            var mySqlTriggerListener = new MySqlTriggerListener<T>(this._connectionString, this._tableName, this._leasesTableName, userFunctionId, context.Executor, this._mysqlOptions, this._logger, this._configuration);
            return Task.FromResult<IListener>(mySqlTriggerListener);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new MySqlTriggerParameterDescriptor
            {
                Name = this._parameter.Name,
                Type = "MySqlTrigger",
                TableName = this._tableName,
            };
        }

        /// <summary>
        /// Returns an ID that uniquely identifies the user function.
        ///
        /// We call the WEBSITE_SITE_NAME from the configuration and use that to create the hash of the
        /// user function id. Appending another hash of class+method in here ensures that if there
        /// are multiple user functions within the same process and tracking the same MySQL table, then each one of them
        /// gets a separate view of the table changes.
        /// </summary>
        private string GetUserFunctionIdAsync()
        {
            // Using read-only App name for the hash https://learn.microsoft.com/en-us/azure/app-service/reference-app-settings?tabs=kudu%2Cdotnet#app-environment
            string websiteName = MySqlBindingUtilities.GetWebSiteName(this._configuration);

            var methodInfo = (MethodInfo)this._parameter.Member;
            // Get the function name from FunctionName attribute for .NET functions and methodInfo.Name for non .Net
            string functionName = ((FunctionNameAttribute)methodInfo.GetCustomAttribute(typeof(FunctionNameAttribute)))?.Name ?? $"{methodInfo.Name}";

            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(websiteName + functionName));
                return new Guid(hash.Take(16).ToArray()).ToString("N").Substring(0, 16);
            }
        }
    }
}
