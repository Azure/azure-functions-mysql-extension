// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    internal class MySqlScalerProvider : IScaleMonitorProvider, ITargetScalerProvider
    {
        private readonly MySqlTriggerScaleMonitor _scaleMonitor;
        private readonly MySqlTriggerTargetScaler _targetScaler;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlScalerProvider"/> class.
        /// Use for obtaining scale metrics by scale controller.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="triggerMetadata"></param>
        public MySqlScalerProvider(IServiceProvider serviceProvider, TriggerMetadata triggerMetadata)
        {
            IConfiguration config = serviceProvider.GetService<IConfiguration>();
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("MySql"));
            MySqlMetaData MySqlMetadata = JsonConvert.DeserializeObject<MySqlMetaData>(triggerMetadata.Metadata.ToString());
            MySqlMetadata.ResolveProperties(serviceProvider.GetService<INameResolver>());
            var userTable = new MySqlObject(MySqlMetadata.TableName);
            string connectionString = MySqlBindingUtilities.GetConnectionString(MySqlMetadata.ConnectionStringSetting, config);
            IOptions<MySqlOptions> options = serviceProvider.GetService<IOptions<MySqlOptions>>();
            int configOptionsMaxChangesPerWorker = options.Value.MaxChangesPerWorker;
            int configAppSettingsMaxChangesPerWorker = config.GetValue<int>(MySqlTriggerConstants.ConfigKey_MySqlTrigger_MaxChangesPerWorker);
            // Override the maxChangesPerWorker value from config if the value is set in the trigger appsettings
            int maxChangesPerWorker = configAppSettingsMaxChangesPerWorker != 0 ? configAppSettingsMaxChangesPerWorker : configOptionsMaxChangesPerWorker != 0 ? configOptionsMaxChangesPerWorker : MySqlOptions.DefaultMaxChangesPerWorker;
            string userDefinedLeasesTableName = MySqlMetadata.LeasesTableName;
            string userFunctionId = MySqlMetadata.UserFunctionId;

            this._scaleMonitor = new MySqlTriggerScaleMonitor(userFunctionId, userTable, userDefinedLeasesTableName, connectionString, maxChangesPerWorker, logger);
            this._targetScaler = new MySqlTriggerTargetScaler(userFunctionId, userTable, userDefinedLeasesTableName, connectionString, maxChangesPerWorker, logger);
        }

        public IScaleMonitor GetMonitor()
        {
            return this._scaleMonitor;
        }

        public ITargetScaler GetTargetScaler()
        {
            return this._targetScaler;
        }

        internal class MySqlMetaData
        {
            [JsonProperty]
            public string ConnectionStringSetting { get; set; }

            [JsonProperty]
            public string TableName { get; set; }

            [JsonProperty]
            public string LeasesTableName { get; set; }

            [JsonProperty]
            public string UserFunctionId { get; set; }

            public void ResolveProperties(INameResolver resolver)
            {
                if (resolver != null)
                {
                    this.ConnectionStringSetting = resolver.ResolveWholeString(this.ConnectionStringSetting);
                }
            }
        }
    }
}