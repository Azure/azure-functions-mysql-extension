/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package com.function;

import com.microsoft.azure.functions.ExecutionContext;
import com.microsoft.azure.functions.annotation.FunctionName;
import com.microsoft.azure.functions.mysql.annotation.MySqlTrigger;
import com.function.Common.MySqlChangeProduct;
import com.google.gson.Gson;

import java.util.logging.Level;

public class ProductsTriggerLeasesTableName {
    @FunctionName("ProductsTriggerLeasesTableName")
    public void run(
            @MySqlTrigger(
                name = "changes",
                tableName = "Products",
                connectionStringSetting = "MySqlConnectionString",
                leasesTableName = "Leases")
                MySqlChangeProduct[] changes,
            ExecutionContext context) {

        context.getLogger().log(Level.INFO, "MySql Changes: " + new Gson().toJson(changes));
    }
}