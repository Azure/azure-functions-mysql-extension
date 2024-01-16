// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.MySql.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    public static class MySqlBindingExtension
    {
        public static IWebJobsBuilder AddMySql(this IWebJobsBuilder builder, Action<MySqlOptions> configureMySqlOptions = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // builder.Services.TryAddSingleton<MySqlTriggerBindingProvider>();

            builder.AddExtension<MySqlExtensionConfigProvider>().BindOptions<MySqlOptions>();

            if (configureMySqlOptions != null)
            {
#pragma warning disable IDE0001 // Cannot simplify the name here, supressing the warning.
                builder.Services.Configure<MySqlOptions>(configureMySqlOptions);
#pragma warning restore IDE0001
            }

            return builder;
        }
    }
}
