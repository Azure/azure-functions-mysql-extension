# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.

using namespace System.Net

# Trigger binding data passed in via param block
param($Request, $TriggerMetadata)

# Write to the Azure Functions log stream.
Write-Host "PowerShell function with MySql Output Binding processed a request."

# Currently the Powershell worker does not allow empty/null values to be passed through the
# query parameters. We use TriggerMetadata here as a workaround for that issue. 
# Issue link: https://github.com/Azure/azure-functions-powershell-worker/issues/895
$req_query = @{
    "ProductId"= $TriggerMetadata["productId"];
    "Name"= $TriggerMetadata["name"];
    "Cost"= $TriggerMetadata["cost"];
};

# Assign the value we want to pass to the MySql Output binding.
# The -Name value corresponds to the name property in the function.json for the binding
Push-OutputBinding -Name product -Value $req_query

# Assign the value to return as the HTTP response.
# The -Name value matches the name property in the function.json for the binding
Push-OutputBinding -Name response -Value ([HttpResponseContext]@{
    StatusCode = [HttpStatusCode]::OK
    Body = $req_query
})