// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    internal static class MySqlTriggerConstants
    {
        public const string MYSQL_FUNC_CURRENTTIME = "CURRENT_TIMESTAMP";

        public const string UpdateAtColumnName = "az_func_updated_at";

        public const string SchemaName = "az_func_mysql";

        public const string GlobalState = "GlobalState";
        public const string GlobalStateTableName = SchemaName + "." + GlobalState;
        public const string GlobalStateTableUserFunctionIDColumnName = "UserFunctionID";
        public const string GlobalStateTableUserTableIDColumnName = "UserTableID";
        public const string GlobalStateTableLastPolledTimeColumnName = "LastPolledTime";

        public const string SysChangeVersionColumnName = "SYS_CHANGE_VERSION";
        public const string ConfigKey_MySqlTrigger_BatchSize = "MySql_Trigger_BatchSize";
        public const string ConfigKey_MySqlTrigger_MaxBatchSize = "MySql_Trigger_MaxBatchSize";
        public const string ConfigKey_MySqlTrigger_PollingInterval = "MySql_Trigger_PollingIntervalMs";
        public const string ConfigKey_MySqlTrigger_MaxChangesPerWorker = "MySql_Trigger_MaxChangesPerWorker";

        public const string LeasesTableNameFormat = SchemaName + ".`Leases_{0}`"; // function-id could have hypen(-) in name, which needs to be quoted in acute quote(`)
        public const string UserDefinedLeasesTableNameFormat = SchemaName + ".{0}";
        public const string LeasesTableSyncCompletedTime = "_az_func_SyncCompletedTime";
        public const string LeasesTableAttemptCountColumnName = "_az_func_AttemptCount";
        public const string LeasesTableLeaseExpirationTimeColumnName = "_az_func_LeaseExpirationTime";

        /// <summary>
        /// The maximum number of times that we'll attempt to renew a lease be
        /// </summary>
        public const int MaxChangeProcessAttemptCount = 5;
        /// The intialize attempt count from
        public const int InitialValueAttemptCount = 1;

        /// <summary>
        /// The column names that are used in internal state tables and so can't exist in the target table
        /// since that shares column names with the primary keys from each user table being monitored.
        /// </summary>
        public static readonly string[] ReservedColumnNames = new string[]
        {
                    LeasesTableAttemptCountColumnName,
                    LeasesTableLeaseExpirationTimeColumnName
        };

        //list unsupported data types
        public static HashSet<string> UnsupportedColumnDataTypes = new HashSet<string>()
        { "point", "multipoint", "linestring", "multilinestring",
           "polygon", "multipolygon", "geometry", "geometrycollection"
        };
    }
}
