// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.Azure.WebJobs.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <summary>
    /// Represents configuration for <see cref="MySqlBindingExtension"/>.
    /// </summary>
    public class MySqlOptions : IOptionsFormatter
    {
        // NOTE: please ensure the Readme file and other public documentation are also updated if the deafult values
        // are ever changed.
        public const int DefaultPollingIntervalMs = 1000;
        private const int DefaultMinimumPollingIntervalMs = 100;
        /// <summary>
        /// Delay in ms between processing each batch of changes
        /// </summary>
        private int _pollingIntervalMs = DefaultPollingIntervalMs;
        private readonly int _minPollingInterval = DefaultMinimumPollingIntervalMs;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlOptions"/> class.
        /// </summary>
        public MySqlOptions()
        {

        }

        /// <summary>
        /// Gets or sets the longest period of time to wait before checking for next batch of changes on the server.
        /// </summary>
        public int PollingIntervalMs
        {
            get => this._pollingIntervalMs;

            set
            {
                if (value < this._minPollingInterval)
                {
                    string message = string.Format(System.Globalization.CultureInfo.CurrentCulture,
                        "PollingInterval must not be less than {0}Ms.", this._minPollingInterval);
                    throw new ArgumentException(message, nameof(value));
                }

                this._pollingIntervalMs = value;
            }
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        string IOptionsFormatter.Format()
        {
            var options = new JObject
            {
                { nameof(this.PollingIntervalMs), this.PollingIntervalMs }
            };

            return options.ToString(Formatting.Indented);
        }

        internal MySqlOptions Clone()
        {
            var copy = new MySqlOptions
            {
                _pollingIntervalMs = this._pollingIntervalMs
            };
            return copy;
        }
    }

}
