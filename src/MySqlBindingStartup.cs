// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Extensions.MySql;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(MySqlBindingStartup))]

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    public class MySqlBindingStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddMySql();
        }
    }
}
