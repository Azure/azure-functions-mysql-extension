// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
        [InlineData("mydb.Products", "mydb", "`mydb`", "'mydb'", "Products", "`Products`", "'Products'", "mydb.Products", "`mydb`.`Products`")]
        [InlineData("`mydb`.Products", "mydb", "`mydb`", "'mydb'", "Products", "`Products`", "'Products'", "mydb.Products", "`mydb`.`Products`")]
        [InlineData("mydb.`Products`", "mydb", "`mydb`", "'mydb'", "Products", "`Products`", "'Products'", "mydb.Products", "`mydb`.`Products`")]
        [InlineData("`mydb`.`Products`", "mydb", "`mydb`", "'mydb'", "Products", "`Products`", "'Products'", "mydb.Products", "`mydb`.`Products`")]
        [InlineData("Products", "SCHEMA()", "SCHEMA()", "SCHEMA()", "Products", "`Products`", "'Products'", "Products", "`Products`")]
        [InlineData("`Products`", "SCHEMA()", "SCHEMA()", "SCHEMA()", "Products", "`Products`", "'Products'", "Products", "`Products`")]
        [InlineData("`Products'`", "SCHEMA()", "SCHEMA()", "SCHEMA()", "Products'", "`Products'`", "'Products\\''", "Products'", "`Products'`")]
        [InlineData("`Products\\'`", "SCHEMA()", "SCHEMA()", "SCHEMA()", "Products\\'", "`Products\\'`", "'Products\\\\\\''", "Products\\'", "`Products\\'`")]
        [InlineData("`''`", "SCHEMA()", "SCHEMA()", "SCHEMA()", "''", "`''`", "'\\'\\''", "''", "`''`")]
        public void TestMySqlObject(string fullName,
            string expectedSchema,
            string expectedAcuteQuotedSchema,
            string expectedSingleQuotedSchema,
            string expectedName,
            string expectedAcuteQuotedName,
            string expectedSingleQuotedName,
            string expectedFullName,
            string expectedAcuteQuotedFullName)
        {
            var MySqlObj = new MySqlObject(fullName);
            Assert.Equal(expectedSchema, MySqlObj.Schema);
            Assert.Equal(expectedAcuteQuotedSchema, MySqlObj.AcuteQuotedSchema);
            Assert.Equal(expectedSingleQuotedSchema, MySqlObj.SingleQuotedSchema);
            Assert.Equal(expectedName, MySqlObj.Name);
            Assert.Equal(expectedAcuteQuotedName, MySqlObj.AcuteQuotedName);
            Assert.Equal(expectedSingleQuotedName, MySqlObj.SingleQuotedName);
            Assert.Equal(expectedFullName, MySqlObj.FullName);
            Assert.Equal(expectedAcuteQuotedFullName, MySqlObj.AcuteQuotedFullName);
        }

        [Theory]
        [InlineData("'mydb'.'Products'", "Encountered error while parsing object name:")]
        [InlineData("\"mydb\".\"Products\"", "Encountered error while parsing object name:")]
        [InlineData("'Products'", "Encountered error while parsing object name:")]
        public void TestMySqlObjectParseError(string fullName, string expectedErrorMessage)
        {
            string errorMessage = Assert.Throws<InvalidOperationException>(() => new MySqlObject(fullName)).Message;
            Assert.StartsWith(expectedErrorMessage, errorMessage);
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
