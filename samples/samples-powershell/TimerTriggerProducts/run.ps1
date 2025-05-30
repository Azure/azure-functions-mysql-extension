# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.

using namespace System.Net

param($myTimer, $TriggerMetadata)
$executionNumber = 0;
$totalUpserts = 100;

# Write to the Azure Functions log stream.
$start = Get-Date
Write-Host "[QueueTrigger]: $start starting execution $executionNumber. Rows to generate=$totalUpserts."

$products = @()
for ($i = 0; $i -lt $totalUpserts; $i++) {
    $products += [PSCustomObject]@{
        ProductId = $i;
        Name = "product";
        Cost = 100 * $i;
    }
}
$end = Get-Date
$duration = New-TimeSpan -Start $start -End $end

# Assign the value we want to pass to the MySql Output binding. 
# The -Name value corresponds to the name property in the function.json for the binding
Push-OutputBinding -Name products -Value $products

Write-Host "[QueueTrigger]: $end finished execution $queueMessage. Total time to create $totalUpserts rows=$duration."
$executionNumber += 1;