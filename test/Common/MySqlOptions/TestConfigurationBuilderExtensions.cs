﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common
{
    public static class TestConfigurationBuilderExtensions
    {
        /// <summary>
        /// Allows configuration to come from %userprofile%\.azurefunctions\appsettings.tests.json
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <returns>The modified configuration builder.</returns>
        public static IConfigurationBuilder AddTestSettings(this IConfigurationBuilder builder)
        {
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".azurefunctions", "appsettings.tests.json");
            return builder.AddJsonFile(configPath, true);
        }
    }
}