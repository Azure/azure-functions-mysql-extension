// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common
{
    public class ProductDefaultPKAndDifferentColumnOrder
    {
        public int Cost { get; set; }

        public string Name { get; set; }
    }
}