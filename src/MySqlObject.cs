// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.WebJobs.Extensions.MySql
{
    /// <summary>
    /// Helper class for working with MySQL object names.
    /// </summary>
    internal class MySqlObject
    {
        private static readonly string SCHEMA_NAME_FUNCTION = "SCHEMA()";

        /// <summary>
        /// The name of the object
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The name of the object, quoted and escaped with acute quote (`)
        /// </summary>
        public readonly string AcuteQuotedName;
        /// <summary>
        /// The name of the object, quoted and escaped with single quote (')
        /// </summary>
        public readonly string SingleQuotedName;
        /// <summary>
        /// The schema of the object, defaulting to the SCHEMA() function if the full name doesn't include a schema
        /// </summary>
        public readonly string Schema;
        /// <summary>
        /// The name of the object, quoted and escaped with acute quotes (`) if it's not the default SCHEMA() function
        /// </summary>
        public readonly string AcuteQuotedSchema;
        /// <summary>
        /// The name of the object, quoted and escaped with single quotes (') if it's not the default SCHEMA() function
        /// </summary>
        public readonly string SingleQuotedSchema;
        /// <summary>
        /// The full name of the object in the format SCHEMA.NAME (or just NAME if there is no specified schema)
        /// </summary>
        public readonly string FullName;
        /// <summary>
        /// The full name of the object in the format 'SCHEMA.NAME' (or just 'NAME' if there is no specified schema), quoted and escaped with single quotes
        /// </summary>
        /// <remarks>The schema and name are also bracket quoted to avoid issues when there are .'s in the object names</remarks>
        public readonly string QuotedFullName;

        /// <summary>
        /// The full name of the objected in the format `SCHEMA`.`NAME` (or just `NAME` if there is no specified schema), quoted and escaped with square brackets.
        /// </summary>
        public readonly string AcuteQuotedFullName;

        /// <summary>
        /// A MySqlObject which contains information about the name and schema of the given object full name.
        /// </summary>
        /// <param name="fullName">Full name of object, including schema (if it exists).</param>
        /// <exception cref="InvalidOperationException">If the name can't be parsed</exception>
        public MySqlObject(string fullName)
        {
            var visitor = new MySqlObjectNameParser(fullName);

            this.Schema = visitor.schemaName;
            this.AcuteQuotedSchema = (this.Schema == SCHEMA_NAME_FUNCTION) ? this.Schema : this.Schema.AsAcuteQuotedString();
            this.SingleQuotedSchema = (this.Schema == SCHEMA_NAME_FUNCTION) ? this.Schema : this.Schema.AsSingleQuotedString();
            this.Name = visitor.objectName;
            this.AcuteQuotedName = this.Name.AsAcuteQuotedString();
            this.SingleQuotedName = this.Name.AsSingleQuotedString();
            this.FullName = (this.Schema == SCHEMA_NAME_FUNCTION) ? this.Name : $"{this.Schema}.{this.Name}";
            this.AcuteQuotedFullName = this.Schema == SCHEMA_NAME_FUNCTION ? this.Name.AsAcuteQuotedString() : $"{this.Schema.AsAcuteQuotedString()}.{this.Name.AsAcuteQuotedString()}";
        }

        /// <summary>
        /// Get the schema and object name from the SchemaObjectName.
        /// </summary>
        internal class MySqlObjectNameParser
        {
            // followed the reference to validate identifier https://dev.mysql.com/doc/refman/8.0/en/identifiers.html
            // regex expression to match following example expressions
            // 1) `mys`adgasg`chema`.`mytable` 2) `myschema`.mytable 3) myschema.`mytable` 4) myschema.mytable
            // 5) `mytable` 6) mytable
            private const string patternSchemaAndObject = @"(`(?<schema>[\u0001-\u007F\u0080-\uFFFF]+)`|(?<schema>[\w$\u0080-\uFFFF]+))\.(`(?<object>[\u0001-\u007F\u0080-\uFFFF]+)`|(?<object>[\w$\u0080-\uFFFF]+))";
            private const string patternObjectWithoutSchema = @"`(?<object>[\u0001-\u007F\u0080-\uFFFF]+)`|(?<object>^(?!')[\w$]+(?!')$)";

            public string schemaName;
            public string objectName;

            internal MySqlObjectNameParser(string objectFullName)
            {
                Match match = Regex.Match(objectFullName, patternSchemaAndObject);
                if (match.Success)
                {
                    this.schemaName = match.Groups["schema"].Value;
                    this.objectName = match.Groups["object"].Value;
                }
                else
                {
                    match = Regex.Match(objectFullName, patternObjectWithoutSchema);
                    if (match.Success)
                    {
                        this.schemaName = SCHEMA_NAME_FUNCTION;
                        this.objectName = match.Groups["object"].Value;
                    }
                    else
                    {
                        this.schemaName = null;
                        this.objectName = null;
                        // throw error message
                        string errorMessages = "Encountered error(s) while parsing object name:\n";
                        throw new InvalidOperationException(errorMessages);
                    }
                }
            }
        }
    }

}