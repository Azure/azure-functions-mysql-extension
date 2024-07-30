// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Unit
{
    public class MySqlOutputBindingTests
    {
        private static readonly Mock<IConfiguration> config = new();
        private static readonly Mock<ILogger> logger = new();

        [Fact]
        public void TestNullCollectorConstructorArguments()
        {
            var arg = new MySqlAttribute(string.Empty, "MySqlConnectionString");
            Assert.Throws<ArgumentNullException>(() => new MySqlAsyncCollector<string>(config.Object, null, logger.Object));
            Assert.Throws<ArgumentNullException>(() => new MySqlAsyncCollector<string>(null, arg, logger.Object));
        }

        [Theory]
        [InlineData("dbo.Products", "dbo", "'dbo'", "Products", "'Products'", "dbo.Products", "'[dbo].[Products]'")] // Simple full name
        [InlineData("Products", "SCHEMA()", "SCHEMA()", "Products", "'Products'", "Products", "'[Products]'")] // Simple no schema
        [InlineData("[dbo].[Products]", "dbo", "'dbo'", "Products", "'Products'", "dbo.Products", "'[dbo].[Products]'")] // Simple full name bracket quoted
        [InlineData("[dbo].Products", "dbo", "'dbo'", "Products", "'Products'", "dbo.Products", "'[dbo].[Products]'")] // Simple full name only schema bracket quoted
        [InlineData("dbo.[Products]", "dbo", "'dbo'", "Products", "'Products'", "dbo.Products", "'[dbo].[Products]'")] // Simple full name only name bracket quoted
        [InlineData("[My'Schema].[Prod'ucts]", "My'Schema", "'My''Schema'", "Prod'ucts", "'Prod''ucts'", "My'Schema.Prod'ucts", "'[My''Schema].[Prod''ucts]'")] // Full name with single quotes in schema and name
        [InlineData("[My]]Schema].[My]]Object]", "My]Schema", "'My]Schema'", "My]Object", "'My]Object'", "My]Schema.My]Object", "'[My]]Schema].[My]]Object]'")] // Full name with brackets in schema and name
        public void TestMySqlObject(string fullName,
            string expectedSchema,
            string expectedQuotedSchema,
            string expectedTableName,
            string expectedSchemaTableName,
            string expectedFullName,
            string expectedQuotedFullName)
        {
            var MySqlObj = new MySqlObject(fullName);
            Assert.Equal(expectedSchema, MySqlObj.Schema);
            Assert.Equal(expectedQuotedSchema, MySqlObj.QuotedSchema);
            Assert.Equal(expectedTableName, MySqlObj.Name);
            Assert.Equal(expectedSchemaTableName, MySqlObj.QuotedName);
            Assert.Equal(expectedFullName, MySqlObj.FullName);
            Assert.Equal(expectedQuotedFullName, MySqlObj.QuotedFullName);
        }

        [Theory]
        [InlineData("myschema.my'table", "Expected but did not find a closing quotation mark after the character string 'table.\n")]
        [InlineData("my'schema.mytable", "Expected but did not find a closing quotation mark after the character string 'schema.mytable.\n")]
        [InlineData("schema.mytable", "Incorrect syntax near 'schema'.\n")] // 'schema' is a keyword and needs to be bracket-quoted to be used as the schema name.
        [InlineData("myschema.table", "Incorrect syntax near '.'.\n")] // 'table' is a keyword and needs to be bracket-quoted to be used as the table name.
        public void TestMySqlObjectParseError(string fullName, string expectedError)
        {
            string expectedErrorMessage = "Encountered error(s) while parsing schema and object name:\n" + expectedError;
            string errorMessage = Assert.Throws<InvalidOperationException>(() => new MySqlObject(fullName)).Message;
            Assert.Equal(expectedErrorMessage, errorMessage);
        }

        [Theory]
        [InlineData("columnName", "`columnName`")]
        [InlineData("column`Name", "`column``Name`")]
        [InlineData("col`umn`Name", "`col`umn``Name`")]
        public void TestAsAcuteQuotedString(string s, string expectedResult)
        {
            string result = s.AsAcuteQuotedString();
            Assert.Equal(expectedResult, result);
        }
    }
}
