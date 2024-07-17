// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Unit
{
    public class MySqlOptionsTests
    {
        [Fact]
        public void Constructor_Defaults()
        {
            var options = new MySqlOptions();

            Assert.Equal(100, options.MaxBatchSize);
            Assert.Equal(1000, options.PollingIntervalMs);
            Assert.Equal(1000, options.MaxChangesPerWorker);
        }

        [Fact]
        public void NewOptions_CanGetAndSetValue()
        {
            var options = new MySqlOptions();

            Assert.Equal(100, options.MaxBatchSize);
            options.MaxBatchSize = 200;
            Assert.Equal(200, options.MaxBatchSize);

            Assert.Equal(1000, options.PollingIntervalMs);
            options.PollingIntervalMs = 2000;
            Assert.Equal(2000, options.PollingIntervalMs);

            Assert.Equal(1000, options.MaxChangesPerWorker);
            options.MaxChangesPerWorker = 200;
            Assert.Equal(200, options.MaxChangesPerWorker);
        }

        [Fact]
        public void JsonSerialization()
        {
            var jo = new JObject
            {
                { "MaxBatchSize", 10 },
                { "PollingIntervalMs", 2000 },
                { "MaxChangesPerWorker", 10}
            };
            MySqlOptions options = jo.ToObject<MySqlOptions>();

            Assert.Equal(10, options.MaxBatchSize);
            Assert.Equal(2000, options.PollingIntervalMs);
            Assert.Equal(10, options.MaxChangesPerWorker);
        }
    }
}
