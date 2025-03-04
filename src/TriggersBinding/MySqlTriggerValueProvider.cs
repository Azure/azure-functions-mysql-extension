﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <summary>
    /// Provider for value that will be passed as argument to the triggered function.
    /// </summary>
    internal class MySqlTriggerValueProvider : IValueProvider
    {
        private readonly object _value;
        private readonly string _tableName;
        private readonly Type _parameterType;
        private readonly bool _isString;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTriggerValueProvider"/> class.
        /// </summary>
        /// <param name="parameterType">Type of the trigger parameter</param>
        /// <param name="value">Value of the trigger parameter</param>
        /// <param name="tableName">Name of the user table</param>
        public MySqlTriggerValueProvider(Type parameterType, object value, string tableName)
        {
            this._parameterType = parameterType;
            this._value = value;
            this._tableName = tableName;
            this._isString = parameterType == typeof(string);
        }

        /// <summary>
        /// Gets the trigger argument value.
        /// </summary>
        public Type Type
        {
            get
            {
                if (this._isString)
                {
                    return typeof(IReadOnlyCollection<JObject>);
                }
                return this._parameterType;
            }
        }

        /// <summary>
        /// Returns value of the trigger argument.
        /// </summary>
        public Task<object> GetValueAsync()
        {
            if (this._isString)
            {
                return Task.FromResult<object>(Utils.JsonSerializeObject(this._value));
            }

            return Task.FromResult(this._value);
        }

        public string ToInvokeString()
        {
            return this._tableName;
        }
    }
}
