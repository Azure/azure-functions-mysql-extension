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

            Assert.Equal(1000, options.PollingIntervalMs);
        }

        [Fact]
        public void NewOptions_CanGetAndSetValue()
        {
            var options = new MySqlOptions();

            Assert.Equal(1000, options.PollingIntervalMs);
            options.PollingIntervalMs = 2000;
            Assert.Equal(2000, options.PollingIntervalMs);
        }

        [Fact]
        public void JsonSerialization()
        {
            var jo = new JObject
            {
                { "PollingIntervalMs", 2000 }
            };
            MySqlOptions options = jo.ToObject<MySqlOptions>();

            Assert.Equal(2000, options.PollingIntervalMs);
        }
    }
}
