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
        [InlineData("mydb.Products", "mydb", "`mydb`", "Products", "`Products`", "mydb.Products", "`mydb`.`Products`")]
        [InlineData("`mydb`.Products", "mydb", "`mydb`", "Products", "`Products`", "mydb.Products", "`mydb`.`Products`")]
        [InlineData("mydb.`Products`", "mydb", "`mydb`", "Products", "`Products`", "mydb.Products", "`mydb`.`Products`")]
        [InlineData("`mydb`.`Products`", "mydb", "`mydb`", "Products", "`Products`", "mydb.Products", "`mydb`.`Products`")]
        [InlineData("Products", "SCHEMA()", "SCHEMA()", "Products", "`Products`", "Products", "`Products`")]
        [InlineData("`Products`", "SCHEMA()", "SCHEMA()", "Products", "`Products`", "Products", "`Products`")]
        [InlineData("`''`", "SCHEMA()", "SCHEMA()", "''", "`''`", "''", "`''`")]
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
        [InlineData("'mydb'.'Products'", "Encountered error(s) while parsing object name:\n")]
        [InlineData("\"mydb\".\"Products\"", "Encountered error(s) while parsing object name:\n")]
        [InlineData("'Products'", "Encountered error(s) while parsing object name:\n")]
        public void TestMySqlObjectParseError(string fullName, string expectedErrorMessage)
        {
            string errorMessage = Assert.Throws<InvalidOperationException>(() => new MySqlObject(fullName)).Message;
            Assert.Equal(expectedErrorMessage, errorMessage);
        }

        [Theory]
        [InlineData("columnName", "`columnName`")]
        [InlineData("mydb.tablename", "`mydb.tablename`")]
        public void TestAsAcuteQuotedString(string s, string expectedResult)
        {
            string result = s.AsAcuteQuotedString();
            Assert.Equal(expectedResult, result);
        }
    }
}
