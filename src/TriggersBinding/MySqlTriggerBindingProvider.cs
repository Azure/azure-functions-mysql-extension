// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <summary>
    /// Provider class for MySQL trigger parameter binding.
    /// </summary>
    internal sealed class MySqlTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IOptions<MySqlOptions> _mysqlOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTriggerBindingProvider"/> class.
        /// </summary>
        /// <param name="configuration">Configuration to retrieve settings from</param>
        /// <param name="loggerFactory">Used to create logger instance</param>
        /// <param name="mysqlOptions"></param>
        public MySqlTriggerBindingProvider(IConfiguration configuration, ILoggerFactory loggerFactory, IOptions<MySqlOptions> mysqlOptions)
        {
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._mysqlOptions = mysqlOptions ?? throw new ArgumentNullException(nameof(mysqlOptions));
            this._logger = loggerFactory?.CreateLogger(LogCategories.CreateTriggerCategory("MySql")) ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Creates MySQL trigger parameter binding.
        /// </summary>
        /// <param name="context">Contains information about trigger parameter and trigger attributes</param>
        /// <exception cref="ArgumentNullException">Thrown if the context is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="MySqlTriggerAttribute" /> is bound to an invalid parameter type.</exception>
        /// <returns>
        /// Null if the user function parameter does not have <see cref="MySqlTriggerAttribute" /> applied. Otherwise returns an
        /// <see cref="MySqlTriggerBinding{T}" /> instance, where T is the user-defined POCO type.
        /// </returns>
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {

            }

            ParameterInfo parameter = context.Parameter;
            MySqlTriggerAttribute attribute = parameter.GetCustomAttribute<MySqlTriggerAttribute>(inherit: false);

            // During application startup, the WebJobs SDK calls 'TryCreateAsync' method of all registered trigger
            // binding providers in sequence for each parameter in the user function. A provider that finds the
            // parameter-attribute that it can handle returns the binding object. Rest of the providers are supposed to
            // return null. This binding object later gets used for binding before every function invocation.
            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            Type parameterType = parameter.ParameterType;
            if (!IsValidTriggerParameterType(parameterType))
            {
                throw new InvalidOperationException($"Can't bind MySqlTriggerAttribute to type {parameter.ParameterType}, this is not a supported type.");
            }

            string connectionString = MySqlBindingUtilities.GetConnectionString(attribute.ConnectionStringSetting, this._configuration);

            Type bindingType;
            // Instantiate class 'MySqlTriggerBinding<JObject>' for non .NET In-Proc functions.
            if (parameterType == typeof(string))
            {
                bindingType = typeof(MySqlTriggerBinding<>).MakeGenericType(typeof(JObject));
            }
            else
            {
                // Extract the POCO type 'T' and use it to instantiate class 'SqlTriggerBinding<T>'.
                Type userType = parameter.ParameterType.GetGenericArguments()[0].GetGenericArguments()[0];
                bindingType = typeof(MySqlTriggerBinding<>).MakeGenericType(userType);
            }

            var constructorParameterTypes = new Type[] { typeof(string), typeof(string), typeof(string), typeof(ParameterInfo), typeof(IOptions<MySqlOptions>), typeof(ILogger), typeof(IConfiguration) };
            ConstructorInfo bindingConstructor = bindingType.GetConstructor(constructorParameterTypes);

            object[] constructorParameterValues = new object[] { connectionString, attribute.TableName, attribute.LeasesTableName, parameter, this._mysqlOptions, this._logger, this._configuration };
            var triggerBinding = (ITriggerBinding)bindingConstructor.Invoke(constructorParameterValues);

            return Task.FromResult(triggerBinding);
        }

        /// <summary>
        /// Checks if the type of trigger parameter in the user function is of form string or <see cref="IReadOnlyList{K}" /> whose generic type argument is <see cref="MySqlChange{T}" />.
        /// </summary>
        private static bool IsValidTriggerParameterType(Type type)
        {
            return
                type == typeof(string) ||
                (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) &&
                type.GetGenericArguments()[0].IsGenericType &&
                type.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(MySqlChange<>));
        }
    }
}
