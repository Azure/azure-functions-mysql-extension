// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Unit
{
    public class MySqlOptionsEndToEndTests
    {
        [Fact]
        public void ConfigureOptions_AppliesValuesCorrectly_MySql()
        {
            string extensionPath = "AzureWebJobs:Extensions:MySql";
            var values = new Dictionary<string, string>
            {
                { $"{extensionPath}:MaxBatchSize", "30" },
                { $"{extensionPath}:PollingIntervalMs", "1000" },
                { $"{extensionPath}:MaxChangesPerWorker", "100" },
            };

            MySqlOptions options = TestHelpers.GetConfiguredOptions<MySqlOptions>(b =>
            {
                b.AddMySql();
            }, values);

            // Assert.Equal(30, options.MaxBatchSize);
            Assert.Equal(1000, options.PollingIntervalMs);
            // Assert.Equal(100, options.MaxChangesPerWorker);
        }
    }
}