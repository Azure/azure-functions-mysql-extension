// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Unit
{
    public class MySqlTriggerBindingProviderTests
    {
        /// <summary>
        /// Verifies that null trigger binding is returned if the trigger parameter in user function does not have
        /// <see cref="MySqlTriggerAttribute"/> applied.
        /// </summary>
        [Fact]
        public async Task TryCreateAsync_TriggerParameterWithoutAttribute_ReturnsNullBinding()
        {
            Type parameterType = typeof(IReadOnlyList<MySqlChange<object>>);
            ITriggerBinding binding = await CreateTriggerBindingAsync(parameterType, nameof(UserFunctionWithoutAttribute));
            Assert.Null(binding);
        }

        /// <summary>
        /// Verifies that <see cref="ArgumentException"/> is thrown if the <see cref="MySqlTriggerAttribute"/> applied on
        /// the trigger parameter does not have <see cref="MySqlTriggerAttribute.ConnectionStringSetting"/> property set.
        /// <see cref="MySqlTriggerAttribute"/> attribute applied.
        /// </summary>
        [Fact]
        public async Task TryCreateAsync_MissingConnectionString_ThrowsException()
        {
            Type parameterType = typeof(IReadOnlyList<MySqlChange<object>>);
            Task testCode() { return CreateTriggerBindingAsync(parameterType, nameof(UserFunctionWithoutConnectionString)); }
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentNullException>(testCode);

            Assert.Equal(
                "Value cannot be null. (Parameter 'connectionStringSetting')",
                exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="InvalidOperationException"/> is thrown if the <see cref="MySqlTriggerAttribute"/> is
        /// applied on the trigger parameter of unsupported type.
        /// </summary>
        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(MySqlChange<object>))]
        [InlineData(typeof(IEnumerable<MySqlChange<object>>))]
        [InlineData(typeof(IReadOnlyList<object>))]
        [InlineData(typeof(IReadOnlyList<IReadOnlyList<object>>))]
        public async Task TryCreateAsync_InvalidTriggerParameterType_ThrowsException(Type parameterType)
        {
            Task testCode() { return CreateTriggerBindingAsync(parameterType, nameof(UserFunctionWithAttribute)); }
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(testCode);

            Assert.Equal(
                $"Can't bind MySqlTriggerAttribute to type {parameterType}, this is not a supported type.",
                exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="MySqlTriggerBinding{T}"/> is returned if the <see cref="MySqlTriggerAttribute"/> has all
        /// required properties set and it is applied on the trigger parameter of supported type.
        /// </summary>
        [Fact]
        public async Task TryCreateAsync_ValidTriggerParameterType_ReturnsTriggerBinding()
        {
            Type parameterType = typeof(IReadOnlyList<MySqlChange<object>>);
            ITriggerBinding binding = await CreateTriggerBindingAsync(parameterType, nameof(UserFunctionWithAttribute));
            Assert.IsType<MySqlTriggerBinding<object>>(binding);
        }

        /// <summary>
        /// Verifies that <see cref="MySqlTriggerBinding{T}"/> is returned if the <see cref="MySqlTriggerAttribute"/> has all
        /// required and optional properties set and it is applied on the trigger parameter of supported type.
        /// </summary>
        [Fact]
        public async Task TryCreateAsync_LeasesTableName_ReturnsTriggerBinding()
        {
            Type parameterType = typeof(IReadOnlyList<MySqlChange<object>>);
            ITriggerBinding binding = await CreateTriggerBindingAsync(parameterType, nameof(UserFunctionWithLeasesTableName));
            Assert.IsType<MySqlTriggerBinding<object>>(binding);
        }

        private static async Task<ITriggerBinding> CreateTriggerBindingAsync(Type parameterType, string methodName)
        {
            var provider = new MySqlTriggerBindingProvider(
                Mock.Of<IConfiguration>(c => c["testConnectionStringSetting"] == "testConnectionString"),
                Mock.Of<ILoggerFactory>(f => f.CreateLogger(It.IsAny<string>()) == Mock.Of<ILogger>()),
                Mock.Of<Microsoft.Extensions.Options.IOptions<MySqlOptions>>());

            // Possibly the simplest way to construct a ParameterInfo object.
            ParameterInfo parameter = typeof(MySqlTriggerBindingProviderTests)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(parameterType)
                .GetParameters()[0];

            return await provider.TryCreateAsync(new TriggerBindingProviderContext(parameter, CancellationToken.None));
        }

        private static void UserFunctionWithoutAttribute<T>(T _) { }

        private static void UserFunctionWithoutConnectionString<T>([MySqlTrigger("testTableName", null)] T _) { }

        private static void UserFunctionWithAttribute<T>([MySqlTrigger("testTableName", "testConnectionStringSetting")] T _) { }

        private static void UserFunctionWithLeasesTableName<T>([MySqlTrigger("testTableName", "testConnectionStringSetting", "testLeasesTableName")] T _) { }
    }
}