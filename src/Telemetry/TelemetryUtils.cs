// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Telemetry
{
    public static class TelemetryUtils
    {
        /// <summary>
        /// Adds common connection properties to the property bag for a telemetry event.
        /// </summary>
        /// <param name="props">The property bag to add our connection properties to</param>
        /// <param name="conn">The connection to add properties of</param>
        /// <param name="serverProperties">Server Properties of the target MySql Server queried for a given connection.</param>
        internal static void AddConnectionProps(this IDictionary<TelemetryPropertyName, string> props, MySqlConnection conn, ServerProperties serverProperties)
        {
            props.Add(TelemetryPropertyName.ServerVersion, conn.ServerVersion);
            if (!string.IsNullOrEmpty(serverProperties?.Version))
            {
                props.Add(TelemetryPropertyName.Version, serverProperties.Version);
            }
            if (!string.IsNullOrEmpty(serverProperties?.Version_Comment))
            {
                props.Add(TelemetryPropertyName.Version_Comment, serverProperties.Version_Comment);
            }
        }

        /// <summary>
        /// Returns a dictionary with common connection properties for the specified connection.
        /// </summary>
        /// <param name="conn">The connection to get properties of</param>
        /// <param name="serverProperties">The Version and Version_Comment of the target MySql Server</param>
        /// <returns>The property dictionary</returns>
        internal static Dictionary<TelemetryPropertyName, string> AsConnectionProps(this MySqlConnection conn, ServerProperties serverProperties)
        {
            var props = new Dictionary<TelemetryPropertyName, string>();
            props.AddConnectionProps(conn, serverProperties);
            return props;
        }
    }
}