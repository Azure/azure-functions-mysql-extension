// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Azure.WebJobs.Host.Scale;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    public static class MySqlBindingExtension
    {
        public static IWebJobsBuilder AddMySql(this IWebJobsBuilder builder, Action<MySqlOptions> configureMySqlOptions = null)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Services.TryAddSingleton<MySqlTriggerBindingProvider>();

            builder.AddExtension<MySqlExtensionConfigProvider>().BindOptions<MySqlOptions>();

            if (configureMySqlOptions != null)
            {
                builder.Services.Configure(configureMySqlOptions);
            }

            return builder;
        }
        internal static IWebJobsBuilder AddMySqlScaleForTrigger(this IWebJobsBuilder builder, TriggerMetadata triggerMetadata)
        {
            IServiceProvider serviceProvider = null;
            var scalerProvider = new Lazy<MySqlScalerProvider>(() => new MySqlScalerProvider(serviceProvider, triggerMetadata));
            builder.Services.AddSingleton((Func<IServiceProvider, IScaleMonitorProvider>)delegate (IServiceProvider resolvedServiceProvider)
            {
                serviceProvider ??= resolvedServiceProvider;
                return scalerProvider.Value;
            });
            builder.Services.AddSingleton((Func<IServiceProvider, ITargetScalerProvider>)delegate (IServiceProvider resolvedServiceProvider)
            {
                serviceProvider ??= resolvedServiceProvider;
                return scalerProvider.Value;
            });
            return builder;
        }
    }
}
