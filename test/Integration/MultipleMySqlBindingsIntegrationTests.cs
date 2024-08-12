// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{
    [Collection(IntegrationTestsCollection.Name)]
    [LogTestName]
    public class MultipleMySqlBindingsIntegrationTests : IntegrationTestBase
    {

        public MultipleMySqlBindingsIntegrationTests(ITestOutputHelper output) : base(output)
        {
        }

        /// <summary>
        /// Tests a function with both an input and output binding.
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async void GetAndAddProductsTest(SupportedLanguages lang)
        {
            // Insert 10 rows to Products table
            Product[] products = GetProductsWithSameCost(10, 100);
            this.InsertProducts(products);

            // Run the function
            await this.SendInputRequest("getandaddproducts/100", "", TestUtils.GetPort(lang));

            // Verify that the 10 rows in Products were upserted to ProductsWithIdentity
            Assert.Equal(10, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(1) FROM ProductsWithIdentity")));
        }
    }
}
