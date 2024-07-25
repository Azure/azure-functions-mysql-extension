// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    internal static class MySqlTriggerConstants
    {
        public const string SchemaName = "az_func_mysql";
        public const string GlobalStateTableName = SchemaName + ".GlobalState";
        public const string GlobalStateTableVersionColumnName = "LastPolledTime";
        public const string UpdateAtColumnName = "updated_at";
        public const string SysChangeVersionColumnName = "SYS_CHANGE_VERSION";
        public const string LastAccessTimeColumnName = "LastAccessTime";
        public const string ConfigKey_MySqlTrigger_PollingInterval = "MySql_Trigger_PollingIntervalMs";

        public const string LeasesTableNameFormat = SchemaName + ".Leases_{0}";
        public const string UserDefinedLeasesTableNameFormat = SchemaName + ".{0}";
        public const string LeasesTableChangeVersionColumnName = "_az_func_ChangeVersion";
        public const string LeasesTableAttemptCountColumnName = "_az_func_AttemptCount";
        public const string LeasesTableLeaseExpirationTimeColumnName = "_az_func_LeaseExpirationTime";
    }
}
