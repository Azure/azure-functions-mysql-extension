// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.InputBindingSamples;
using Microsoft.Azure.WebJobs.Extensions.MySql.Samples.Common;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Integration
{
    [Collection(IntegrationTestsCollection.Name)]
    [LogTestName]
    public class MySqlInputBindingIntegrationTests : IntegrationTestBase
    {

        public MySqlInputBindingIntegrationTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [MySqlInlineData(0, 100)]
        [MySqlInlineData(1, -500)]
        [MySqlInlineData(100, 500)]
        public async void GetProductsTest(int n, int cost, SupportedLanguages lang)
        {
            // Generate T-SQL to insert n rows of data with cost
            Product[] products = GetProductsWithSameCost(n, cost);
            this.InsertProducts(products);

            // Run the function
            HttpResponseMessage response = await this.SendInputRequest("getproducts", cost.ToString(), TestUtils.GetPort(lang));

            // Verify result
            string actualResponse = await response.Content.ReadAsStringAsync();
            Product[] actualProductResponse = Utils.JsonDeserializeObject<Product[]>(actualResponse);

            Assert.Equal(products, actualProductResponse);
        }

        [Theory]
        [MySqlInlineData(0, 99)]
        [MySqlInlineData(1, -999)]
        [MySqlInlineData(100, 999)]
        public async void GetProductsStoredProcedureTest(int n, int cost, SupportedLanguages lang)
        {
            // Generate T-SQL to insert n rows of data with cost
            Product[] products = GetProductsWithSameCost(n, cost);
            this.InsertProducts(products);

            // Run the function
            HttpResponseMessage response = await this.SendInputRequest("getproducts-storedprocedure", cost.ToString(), TestUtils.GetPort(lang));

            // Verify result
            string actualResponse = await response.Content.ReadAsStringAsync();
            Product[] actualProductResponse = Utils.JsonDeserializeObject<Product[]>(actualResponse);

            Assert.Equal(products, actualProductResponse);
        }

        [Theory]
        [MySqlInlineData(0, 0)]
        [MySqlInlineData(1, 20)]
        [MySqlInlineData(100, 1000)]
        public async void GetProductsNameEmptyTest(int n, int cost, SupportedLanguages lang)
        {
            // Add a bunch of noise data
            this.InsertProducts(GetProductsWithSameCost(n * 2, cost));

            // Now add the actual test data
            Product[] products = GetProductsWithSameCostAndName(n, cost, "", n * 2);
            this.InsertProducts(products);

            Assert.Equal(n, Convert.ToInt32(this.ExecuteScalar($"select count(1) from Products where name = '' and cost = {cost}")));

            // Run the function
            HttpResponseMessage response = await this.SendInputRequest("getproducts-nameempty", cost.ToString(), TestUtils.GetPort(lang));

            // Verify result
            string actualResponse = await response.Content.ReadAsStringAsync();
            Product[] actualProductResponse = Utils.JsonDeserializeObject<Product[]>(actualResponse);

            Assert.Equal(products, actualProductResponse);
        }

        [Theory]
        [MySqlInlineData()]
        public async void GetProductsByCostTest(SupportedLanguages lang)
        {
            // Generate T-SQL to insert n rows of data with cost
            Product[] products = GetProducts(3, 100);
            this.InsertProducts(products);
            Product[] productsWithCost100 = GetProducts(1, 100);

            // Run the function
            HttpResponseMessage response = await this.SendInputRequest("getproductsbycost", "", TestUtils.GetPort(lang));

            // Verify result
            string actualResponse = await response.Content.ReadAsStringAsync();
            Product[] actualProductResponse = Utils.JsonDeserializeObject<Product[]>(actualResponse);

            Assert.Equal(productsWithCost100, actualProductResponse);
        }

        [Theory]
        [MySqlInlineData()]
        public async void GetProductNamesViewTest(SupportedLanguages lang)
        {
            // Insert one row of data into Product table
            Product[] products = GetProductsWithSameCost(1, 100);
            this.InsertProducts(products);

            // Run the function that queries from the ProductName view
            HttpResponseMessage response = await this.SendInputRequest("getproduct-namesview", "", TestUtils.GetPort(lang));

            // Verify result
            string expectedResponse = /*lang=json,strict*/ "[{\"name\":\"test\"}]";
            string actualResponse = await response.Content.ReadAsStringAsync();

            Assert.Equal(expectedResponse, TestUtils.CleanJsonString(actualResponse), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that serializing an item with various data types and different cultures works when using IAsyncEnumerable
        /// </summary>
        [Theory]
        [MySqlInlineData("en-US")]
        [MySqlInlineData("it-IT")]
        [UnsupportedLanguages(SupportedLanguages.JavaScript, SupportedLanguages.PowerShell, SupportedLanguages.Java, SupportedLanguages.Python)] // IAsyncEnumerable is only available in C#
        public async void GetProductsColumnTypesSerializationAsyncEnumerableTest(string culture, SupportedLanguages lang)
        {
            this.StartFunctionHost(nameof(GetProductsColumnTypesSerializationAsyncEnumerable), lang, true);

            string datetime = "2024-08-10 12:40:15";
            ProductColumnTypes[] expectedResponse = Utils.JsonDeserializeObject<ProductColumnTypes[]>(/*lang=json,strict*/ "[{\"ProductId\":999,\"BigInt\":999,\"Bit\":true,\"DecimalType\":1.2345,\"Numeric\":1.2345,\"SmallInt\":1,\"TinyInt\":1,\"FloatType\":0.1,\"Real\":0.1,\"Date\":\"2024-08-10\",\"Datetime\":\"2024-08-10 12:40:15\",\"Time\":\"12:40:15\",\"CharType\":\"test\",\"Varchar\":\"test\",\"Nchar\":\"test\",\"Nvarchar\":\"test\",\"Binary\":\"dGVzdA==\",\"Varbinary\":\"test\"}]");

            this.ExecuteNonQuery("INSERT INTO ProductsColumnTypes VALUES (" +
                "999, " + // ProductId,
                "999, " + // BigInt
                "1, " + // Bit
                "1.2345, " + // DecimalType
                "1.2345, " + // Numeric
                "1, " + // SmallInt
                "1, " + // TinyInt
                ".1, " + // FloatType
                ".1, " + // Real
                $"DATE('{datetime}')," +
                $"'{datetime}'," +
                $"TIME('{datetime}')," +
                "'test', " + // CharType
                "'test', " + // Varchar
                "'test', " + // Nchar
                "'test', " +  // Nvarchar
                "0x9fad, " + // Binary
                "'test')"); // Varbinary

            HttpResponseMessage response = await this.SendInputRequest("getproducts-columntypesserializationasyncenumerable", $"?culture={culture}");
            // We expect the datetime and datetime2 fields to be returned in UTC format
            string actualResponse = await response.Content.ReadAsStringAsync();
            ProductColumnTypes[] actualProductResponse = Utils.JsonDeserializeObject<ProductColumnTypes[]>(actualResponse);
            Assert.Equal(expectedResponse, actualProductResponse);
        }

        /// <summary>
        /// Verifies that serializing an item with various data types works as expected
        /// </summary>
        [Theory]
        [MySqlInlineData()]
        public async void GetProductsColumnTypesSerializationTest(SupportedLanguages lang)
        {
            string datetime = "2024-08-10 12:40:15";
            this.ExecuteNonQuery("INSERT INTO ProductsColumnTypes VALUES (" +
                "999, " + // ProductId,
                "999, " + // BigInt
                "1, " + // Bit
                "1.2345, " + // DecimalType
                "1.2345, " + // Numeric
                "1, " + // SmallInt
                "1, " + // TinyInt
                ".1, " + // FloatType
                ".1, " + // Real
                $"DATE('{datetime}')," +
                $"'{datetime}'," +
                $"TIME('{datetime}')," +
                "'test', " + // CharType
                "'test', " + // Varchar
                "'test', " + // Nchar
                "'test', " +  // Nvarchar
                "0x9fad, " + // Binary
                "'test')"); // Varbinary

            HttpResponseMessage response = await this.SendInputRequest("getproducts-columntypesserialization", "", TestUtils.GetPort(lang, true));
            // We expect the date fields to be returned in UTC format
            ProductColumnTypes[] expectedResponse = Utils.JsonDeserializeObject<ProductColumnTypes[]>(/*lang=json,strict*/ "[{\"ProductId\":999,\"BigInt\":999,\"Bit\":true,\"DecimalType\":1.2345,\"Numeric\":1.2345,\"SmallInt\":1,\"TinyInt\":1,\"FloatType\":0.1,\"Real\":0.1,\"Date\":\"2024-08-10\",\"Datetime\":\"2024-08-10 12:40:15\",\"Time\":\"12:40:15\",\"CharType\":\"test\",\"Varchar\":\"test\",\"Nchar\":\"test\",\"Nvarchar\":\"test\",\"Binary\":\"dGVzdA==\",\"Varbinary\":\"test\"}]");
            string actualResponse = await response.Content.ReadAsStringAsync();
            ProductColumnTypes[] actualProductResponse = Utils.JsonDeserializeObject<ProductColumnTypes[]>(actualResponse);
            Assert.Equal(expectedResponse, actualProductResponse);
        }
    }
}