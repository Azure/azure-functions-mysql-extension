// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.OutputBindingSamples;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{
    [Collection(IntegrationTestsCollection.Name)]
    [LogTestName]
    public class MySqlOutputBindingIntegrationTests : IntegrationTestBase
    {

        public MySqlOutputBindingIntegrationTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [MySqlInlineData(1, "Test", 5)]
        [MySqlInlineData(0, "", 0)]
        [MySqlInlineData(-500, "ABCD", 580)]
        public async Task AddProductTest(int id, string name, int cost, SupportedLanguages lang)
        {
            var query = new Dictionary<string, object>()
            {
                { "ProductId", id },
                { "Name", name },
                { "Cost", cost }
            };

            await this.SendOutputPostRequest("addproduct", Utils.JsonSerializeObject(query), TestUtils.GetPort(lang));

            // Verify result
            Assert.Equal(name, Convert.ToString(this.ExecuteScalar($"select Name from Products where ProductId={id}")));
            Assert.Equal(cost, Convert.ToInt32(this.ExecuteScalar($"select cost from Products where ProductId={id}")));
        }

        [Theory]
        [MySqlInlineData(1, "Test", 5)]
        [MySqlInlineData(0, "", 0)]
        [MySqlInlineData(-500, "ABCD", 580)]
        // Currently Java functions return null when the parameter for name is an empty string
        [UnsupportedLanguages(SupportedLanguages.Java)]
        public async Task AddProductParamsTest(int id, string name, int cost, SupportedLanguages lang)
        {
            var query = new Dictionary<string, string>()
            {
                { "productId", id.ToString() },
                { "name", name },
                { "cost", cost.ToString() }
            };

            await this.SendOutputGetRequest("addproduct-params", query, TestUtils.GetPort(lang));

            // Verify result
            Assert.Equal(name, Convert.ToString(this.ExecuteScalar($"select Name from Products where ProductId={id}")));
            Assert.Equal(cost, Convert.ToInt32(this.ExecuteScalar($"select cost from Products where ProductId={id}")));
        }

        [Theory]
        [MySqlInlineData()]
        public async Task AddProductArrayTest(SupportedLanguages lang)
        {
            // First insert some test data
            this.ExecuteNonQuery("INSERT INTO Products (ProductId, Name, Cost) VALUES (1, 'test', 100)");
            this.ExecuteNonQuery("INSERT INTO Products (ProductId, Name, Cost) VALUES (2, 'test', 100)");
            this.ExecuteNonQuery("INSERT INTO Products (ProductId, Name, Cost) VALUES (3, 'test', 100)");

            Product[] prods = new[]
            {
                new Product()
                {
                    ProductId = 1,
                    Name = "Cup",
                    Cost = 2
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Glasses",
                    Cost = 12
                }
            };

            await this.SendOutputPostRequest("addproducts-array", Utils.JsonSerializeObject(prods), TestUtils.GetPort(lang));

            // Function call changes first 2 rows to (1, 'Cup', 2) and (2, 'Glasses', 12)
            Assert.Equal(1, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(1) FROM Products WHERE Cost = 100")));
            Assert.Equal(2, Convert.ToInt32(this.ExecuteScalar("SELECT Cost FROM Products WHERE ProductId = 1")));
            Assert.Equal(2, Convert.ToInt32(this.ExecuteScalar("SELECT ProductId FROM Products WHERE Cost = 12")));
        }

        /// <summary>
        /// Test compatibility with converting various data types to their respective
        /// MySql server types.
        /// </summary>
        /// <param name="lang">The language to run the test against</param>
        [Theory]
        [MySqlInlineData()]
        public async Task AddProductColumnTypesTest(SupportedLanguages lang)
        {
            var queryParameters = new Dictionary<string, string>()
            {
                { "productId", "999" }
            };

            await this.SendOutputGetRequest("addproduct-columntypes", queryParameters, TestUtils.GetPort(lang, true));

            // If we get here then the test is successful - an exception will be thrown if there were any problems
        }

        [Theory]
        [MySqlInlineData()]
        [UnsupportedLanguages(SupportedLanguages.JavaScript, SupportedLanguages.PowerShell, SupportedLanguages.Java, SupportedLanguages.OutOfProc, SupportedLanguages.Python)] // Collectors are only available in C#
        public async Task AddProductsCollectorTest(SupportedLanguages lang)
        {
            this.StartFunctionHost(nameof(AddProductsCollector), lang);

            // Function should add 5000 rows to the table
            await this.SendOutputGetRequest("addproducts-collector");

            Assert.Equal(5000, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(1) FROM Products")));
        }

        [Theory]
        [MySqlInlineData()]
        public void TimerTriggerProductsTest(SupportedLanguages lang)
        {
            this.StartFunctionHost(nameof(TimerTriggerProducts), lang);

            // Since this function runs on a schedule (every 5 seconds), we don't need to invoke it.
            // We will wait 6 seconds to guarantee that it has been fired at least once, and check that at least 1000 rows of data has been added.
            Thread.Sleep(6000);

            int rowsAdded = Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(1) FROM Products"));
            this.LogOutput($"No of rows added:  ${rowsAdded}");
            Assert.True(rowsAdded >= 1000);
        }

        [Theory]
        [MySqlInlineData()]
        public void AddProductExtraColumnsTest(SupportedLanguages lang)
        {
            // Since ProductExtraColumns has columns that does not exist in the table,
            // no rows should be added to the table.
            Assert.Throws<AggregateException>(() => this.SendOutputGetRequest("addproduct-extracolumns", null, TestUtils.GetPort(lang, true)).Wait());
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM Products")));
        }

        [Theory]
        [MySqlInlineData()]
        public async Task AddProductMissingColumnsTest(SupportedLanguages lang)
        {
            // Even though the ProductMissingColumns object is missing the Cost column,
            // the row should still be added successfully since Cost can be null.
            await this.SendOutputPostRequest("addproduct-missingcolumns", "", TestUtils.GetPort(lang, true));
            Assert.Equal(1, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM Products")));
        }

        [Theory]
        [MySqlInlineData()]
        public void AddProductMissingColumnsNotNullTest(SupportedLanguages lang)
        {
            // Since the MySql table does not allow null for the Cost column,
            // inserting a row without a Cost value should throw an Exception.
            Assert.Throws<AggregateException>(() => this.SendOutputPostRequest("addproduct-missingcolumnsexception", "", TestUtils.GetPort(lang, true)).Wait());
        }

        [Theory]
        [MySqlInlineData()]
        public void AddProductNoPartialUpsertTest(SupportedLanguages lang)
        {
            Assert.Throws<AggregateException>(() => this.SendOutputPostRequest("addproducts-nopartialupsert", "", TestUtils.GetPort(lang, true)).Wait());
            // No rows should be upserted since there was a row with an invalid value
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsNameNotNull")));
        }

        /// <summary>
        /// Tests that for tables with an identity column we are able to insert items.
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async Task AddProductWithIdentity(SupportedLanguages lang)
        {
            this.StartFunctionHost(nameof(AddProductWithIdentityColumn), lang);

            // Identity column (ProductId) is left out for new items
            var query = new Dictionary<string, string>()
            {
                { "name", "MyProduct" },
                { "cost", "1" }
            };
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
            await this.SendOutputGetRequest("addproductwithidentitycolumn", query);
            // Product should have been inserted correctly even without an ID when there's an identity column present
            Assert.Equal(1, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
        }

        /// <summary>
        /// Tests that for tables with an identity column we are able to insert multiple items at once
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async Task AddProductsWithIdentityColumnArray(SupportedLanguages lang)
        {
            this.StartFunctionHost(nameof(AddProductsWithIdentityColumnArray), lang);

            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
            await this.SendOutputGetRequest("addproductswithidentitycolumnarray", null);
            // Multiple items should have been inserted
            Assert.Equal(2, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
        }

        /// <summary>
        /// Tests that for tables with multiple primary columns (including an identity column) we are able to
        /// insert items.
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async Task AddProductWithIdentity_MultiplePrimaryColumns(SupportedLanguages lang)
        {
            var query = new Dictionary<string, string>()
            {
                { "externalId", "101" },
                { "name", "MyProduct" },
                { "cost", "1" }
            };
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithMultiplePrimaryColumnsAndIdentity")));
            await this.SendOutputGetRequest("addproductwithmultipleprimarycolumnsandidentity", query, TestUtils.GetPort(lang));
            // Product should have been inserted correctly even without an ID when there's an identity column present
            Assert.Equal(1, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithMultiplePrimaryColumnsAndIdentity")));
        }

        /// <summary>
        /// Tests that when using a table with an identity column that if the identity column is specified
        /// by the function we handle inserting/updating that correctly.
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async Task AddProductWithIdentity_SpecifyIdentityColumn(SupportedLanguages lang)
        {
            this.StartFunctionHost(nameof(AddProductWithIdentityColumnIncluded), lang);
            var query = new Dictionary<string, string>()
            {
                { "productId", "1" },
                { "name", "MyProduct" },
                { "cost", "1" }
            };
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
            await this.SendOutputGetRequest(nameof(AddProductWithIdentityColumnIncluded), query);
            // New row should have been inserted
            Assert.Equal(1, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
            query = new Dictionary<string, string>()
            {
                { "productId", "1" },
                { "name", "MyProduct2" },
                { "cost", "1" }
            };
            await this.SendOutputGetRequest(nameof(AddProductWithIdentityColumnIncluded), query);
            // Existing row should have been updated
            Assert.Equal(1, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
            Assert.Equal(1, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity WHERE Name='MyProduct2'")));
        }

        /// <summary>
        /// Tests that when using a table with an identity column we can handle a null (missing) identity column
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async Task AddProductWithIdentity_NoIdentityColumn(SupportedLanguages lang)
        {
            this.StartFunctionHost(nameof(AddProductWithIdentityColumnIncluded), lang);
            var query = new Dictionary<string, string>()
            {
                { "name", "MyProduct" },
                { "cost", "1" }
            };
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
            await this.SendOutputGetRequest(nameof(AddProductWithIdentityColumnIncluded), query);
            // New row should have been inserted
            Assert.Equal(1, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
            query = new Dictionary<string, string>()
            {
                { "name", "MyProduct2" },
                { "cost", "1" }
            };
            await this.SendOutputGetRequest(nameof(AddProductWithIdentityColumnIncluded), query);
            // Another new row should have been inserted
            Assert.Equal(2, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithIdentity")));
        }

        /// <summary>
        /// Tests that when using a table with an identity column along with other primary
        /// keys an error is thrown if at least one of the primary keys is missing.
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public void AddProductWithIdentity_MissingPrimaryColumn(SupportedLanguages lang)
        {
            var query = new Dictionary<string, string>()
            {
                // Missing externalId
                { "name", "MyProduct" },
                { "cost", "1" }
            };
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithMultiplePrimaryColumnsAndIdentity")));
            Assert.Throws<AggregateException>(() => this.SendOutputGetRequest("addproductwithmultipleprimarycolumnsandidentity", query, TestUtils.GetPort(lang)).Wait());
            // Nothing should have been inserted
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithMultiplePrimaryColumnsAndIdentity")));
        }

        /// <summary>
        /// Tests that a row is inserted successfully when the object is missing
        /// the primary key column with a default value.
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async Task AddProductWithDefaultPKTest(SupportedLanguages lang)
        {
            var product = new Dictionary<string, object>()
            {
                { "Name", "MyProduct" },
                { "Cost", 1 }
            };
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithDefaultPK")));
            await this.SendOutputPostRequest("addproductwithdefaultpk", Utils.JsonSerializeObject(product), TestUtils.GetPort(lang));
            await this.SendOutputPostRequest("addproductwithdefaultpk", Utils.JsonSerializeObject(product), TestUtils.GetPort(lang));
            Assert.Equal(2, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithDefaultPK")));
            // Should throw error when there is no default PK and the primary key is missing from the user object.
            Assert.Throws<AggregateException>(() => this.SendOutputPostRequest("addproduct", Utils.JsonSerializeObject(product), TestUtils.GetPort(lang)).Wait());
        }

        /// <summary>
        /// Regression test for ensuring that the query type isn't cached
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async Task QueryTypeCachingRegressionTest(SupportedLanguages lang)
        {
            // Start off by inserting an item into the database, which we'll update later
            this.ExecuteNonQuery("INSERT INTO Products VALUES (1, 'test', 100)");
            // Now make a call that is expected to fail. The important part here is that:
            //      1. This and the function below both target the same table "Products"
            //      2. This one will trigger an "insert" query (which ultimately fails due to the incorrect casing, but the table information is still retrieved first)
            Assert.Throws<AggregateException>(() => this.SendOutputGetRequest("addproduct-incorrectcasing", null, TestUtils.GetPort(lang, true)).Wait());
            // Ensure that we have the one expected item
            Assert.True(1 == (int)(long)this.ExecuteScalar("SELECT COUNT(*) FROM Products"), "There should be one item initially");
            var productWithPrimaryKey = new Dictionary<string, object>()
            {
                { "ProductId", 1 },
                { "Name", "MyNewProduct" },
                { "Cost", 100 }
            };
            // Now send an output request that we expect to succeed - specifically one that will result in an update so requires the MERGE statement
            await this.SendOutputPostRequest("addproduct", Utils.JsonSerializeObject(productWithPrimaryKey), TestUtils.GetPort(lang));
            Assert.True(1 == (int)(long)this.ExecuteScalar("SELECT COUNT(*) FROM Products"), "There should be one item at the end");
        }

        /// <summary>
        /// Tests that subsequent upserts work correctly when the object properties are different from the first upsert.
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async Task AddProductWithDifferentPropertiesTest(SupportedLanguages lang)
        {
            var query1 = new Dictionary<string, object>()
            {
                { "ProductId", 0 },
                { "Name", "test" },
                { "Cost", 100 }
            };

            var query2 = new Dictionary<string, object>()
            {
                { "ProductId", 0 },
                { "Name", "test2" }
            };

            await this.SendOutputPostRequest("addproduct", Utils.JsonSerializeObject(query1), TestUtils.GetPort(lang));
            await this.SendOutputPostRequest("addproduct", Utils.JsonSerializeObject(query2), TestUtils.GetPort(lang));

            // Verify result
            Assert.Equal("test2", this.ExecuteScalar($"select Name from Products where ProductId=0"));
        }

        /// <summary>
        /// Tests that when upserting an item with no properties, an error is thrown.
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        // Only the JavaScript function passes an empty JSON to the MySql extension.
        // C#, Java, and Python throw an error while creating the Product object in the function and in PowerShell,
        // the JSON would be passed as {"ProductId": null, "Name": null, "Cost": null}.
        [UnsupportedLanguages(SupportedLanguages.CSharp, SupportedLanguages.Java, SupportedLanguages.OutOfProc, SupportedLanguages.PowerShell, SupportedLanguages.Python)]
        public async Task NoPropertiesThrows(SupportedLanguages lang)
        {
            var foundExpectedMessageSource = new TaskCompletionSource<bool>();
            this.StartFunctionHost(nameof(AddProductParams), lang, false, (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data.Contains("No property values found in item to upsert. If using query parameters, ensure that the casing of the parameter names and the property names match."))
                {
                    foundExpectedMessageSource.SetResult(true);
                }
            });

            var query = new Dictionary<string, string>() { };

            // The upsert should fail since no parameters were passed
            Exception exception = Assert.Throws<AggregateException>(() => this.SendOutputGetRequest("addproduct-params", query).Wait());
            // Verify the message contains the expected error so that other errors don't mistakenly make this test pass
            // Wait 2sec for message to get processed to account for delays reading output
            await foundExpectedMessageSource.Task.TimeoutAfter(TimeSpan.FromMilliseconds(2000), $"Timed out waiting for expected error message");
        }

        /// <summary>
        /// Tests that rows are inserted correctly when the table contains default values or identity columns even if the order of
        /// the properties in the POCO/JSON object is different from the order of the columns in the table.
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async Task AddProductDefaultPKAndDifferentColumnOrderTest(SupportedLanguages lang)
        {
            Assert.Equal(0, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithDefaultPK")));
            await this.SendOutputGetRequest("addproductdefaultpkanddifferentcolumnorder", null, TestUtils.GetPort(lang, true));
            Assert.Equal(1, Convert.ToInt32(this.ExecuteScalar("SELECT COUNT(*) FROM ProductsWithDefaultPK")));
        }
    }
}