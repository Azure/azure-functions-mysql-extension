# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.

using namespace System.Net

# This function uses a MySql input binding to get products from the Products table
# and upsert those products to the ProductsWithIdentity table.
param($Request, $TriggerMetadata, $products)

Push-OutputBinding -Name productsWithIdentity -Value $products

Push-OutputBinding -Name response -Value ([HttpResponseContext]@{
    StatusCode = [System.Net.HttpStatusCode]::OK
    Body = $products
})