// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common
{
    public class FakeTypeLocator(params Type[] types) : ITypeLocator
    {
        private readonly Type[] _types = types;

        public IReadOnlyList<Type> GetTypes()
        {
            return this._types;
        }
    }
}
