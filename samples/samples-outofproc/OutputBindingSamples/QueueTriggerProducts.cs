// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.MySql;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.SamplesOutOfProc.OutputBindingSamples
{
    public static class QueueTriggerProducts
    {
        [Function("QueueTriggerProducts")]
        [MySqlOutput("[dbo].[Products]", "MySqlConnectionString")]
        public static List<Product> Run([QueueTrigger("testqueue")] string queueMessage)
        {
            int totalUpserts = 100;
            List<Product> newProducts = ProductUtilities.GetNewProducts(totalUpserts);
            return newProducts;
        }

    }

}
