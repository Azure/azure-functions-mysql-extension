# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.

using namespace System.Net

# Trigger binding data passed in via param block
param($queueMessage, $TriggerMetadata)
$totalUpserts = 100;
# Write to the Azure Functions log stream.
Write-Host "[QueueTrigger]: $Get-Date starting execution $queueMessage. Rows to generate=$totalUpserts."

# Note that this expects the body to be a JSON object or array of objects 
# which have a property matching each of the columns in the table to upsert to.
$start = Get-Date

$products = @()
for ($i = 0; $i -lt $totalUpserts; $i++) {
    $products += [PSCustomObject]@{
        ProductId = $i;
        Name = "product";
        Cost = 100 * $i;
    }
}
# Assign the value we want to pass to the MySql Output binding. 
# The -Name value corresponds to the name property in the function.json for the binding
Push-OutputBinding -Name products -Value $products
$end = Get-Date
$duration = New-TimeSpan -Start $start -End $end

Write-Host "[QueueTrigger]: $Get-Date finished execution $queueMessage. Total time to create $totalUpserts rows=$duration."