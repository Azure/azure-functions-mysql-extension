// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    internal class MySqlAsyncCollectorBuilder<T> : IConverter<MySqlAttribute, IAsyncCollector<T>>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

#pragma warning disable IDE0290 // Use primary constructor
        public MySqlAsyncCollectorBuilder(IConfiguration configuration, ILogger logger)
#pragma warning restore IDE0290 // Use primary constructor
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        IAsyncCollector<T> IConverter<MySqlAttribute, IAsyncCollector<T>>.Convert(MySqlAttribute attribute)
        {
            return new MySqlAsyncCollector<T>(this._configuration, attribute, this._logger);
        }
    }
}