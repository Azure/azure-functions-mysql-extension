// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Description;
using static Microsoft.Azure.WebJobs.Extensions.MySql.MySqlConverters;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Logging;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <summary>
    /// Exposes MySQL input, output and trigger bindings
    /// </summary>
    [Extension("mysql")]
    internal class MySqlExtensionConfigProvider : IExtensionConfigProvider, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly MySqlTriggerBindingProvider _triggerProvider;
        private MySqlClientListener mysqlClientListener;
        public const string VerboseLoggingSettingName = "AzureFunctions_MySqlBindings_VerboseLogging";

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlExtensionConfigProvider"/> class.
        /// </summary>
        /// <exception cref = "ArgumentNullException" >
        /// Thrown if either parameter is null
        /// </exception>
        public MySqlExtensionConfigProvider(IConfiguration configuration, ILoggerFactory loggerFactory, MySqlTriggerBindingProvider triggerProvider)
        {
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this._triggerProvider = triggerProvider;
        }

        /// <summary>
        /// Initializes the MySQL binding rules
        /// </summary>
        /// <param name="context"> The config context </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if context is null
        /// </exception>
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            ILogger logger = this._loggerFactory.CreateLogger(LogCategories.Bindings);
            // Only enable MySQL Client logging when VerboseLogging is set in the config to avoid extra overhead when the
            // detailed logging it provides isn't needed
            if (this.mysqlClientListener == null && Utils.GetConfigSettingAsBool(VerboseLoggingSettingName, this._configuration))
            {
                this.mysqlClientListener = new MySqlClientListener(logger);
            }
            LogDependentAssemblyVersions(logger);
#pragma warning disable CS0618 // Fine to use this for our stuff
            FluentBindingRule<MySqlAttribute> inputOutputRule = context.AddBindingRule<MySqlAttribute>();
            var converter = new MySqlConverter(this._configuration);
            inputOutputRule.BindToInput(converter);
            inputOutputRule.BindToInput<string>(typeof(MySqlGenericsConverter<string>), this._configuration, logger);
            inputOutputRule.BindToCollector<MySQLObjectOpenType>(typeof(MySqlAsyncCollectorBuilder<>), this._configuration, logger);
            inputOutputRule.BindToInput<OpenType>(typeof(MySqlGenericsConverter<>), this._configuration, logger);

            context.AddBindingRule<MySqlTriggerAttribute>().BindToTrigger(this._triggerProvider);
        }

        private static readonly Assembly[] _dependentAssemblies = {
            typeof(MySqlConnection).Assembly,
            typeof(JsonConvert).Assembly // Newtonsoft.Json
        };

        /// <summary>
        /// Log the versions of important dependent assemblies for troubleshooting support. The Azure Functions host skips checking
        /// versions for most assemblies loaded so to allow customers to bring their own versions of assemblies and not have conflicts,
        /// but this may end up causing issues in our extension so we log the version loaded of some critical dependencies to ensure
        /// the version is expected.
        /// </summary>
        private static void LogDependentAssemblyVersions(ILogger logger)
        {
            foreach (Assembly assembly in _dependentAssemblies)
            {
                try
                {
                    logger.LogDebug($"Using {assembly.GetName().Name} {FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion}");
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Error logging version for assembly {assembly.FullName}. {ex}");
                }

            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
                this.mysqlClientListener?.Dispose();
            }
            // Free native resources.
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

    /// <summary>
    /// Wrapper around OpenType to receive data correctly from output bindings (not as byte[])
    /// This can be used for general "T --> JObject" bindings.
    /// The exact definition here comes from the WebJobs v1.0 Queue binding.
    /// refer https://github.com/Azure/azure-webjobs-sdk/blob/dev/src/Microsoft.Azure.WebJobs.Host/Bindings/OpenType.cs#L390
    /// </summary>
    internal class MySQLObjectOpenType : OpenType.Poco
    {
        // return true when type is an "System.Object" to enable Object binding.
        public override bool IsMatch(Type type, OpenTypeMatchContext context)
        {
            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return false;
            }

            if (type.FullName == "System.Object")
            {
                return true;
            }

            return base.IsMatch(type, context);
        }
    }
}
