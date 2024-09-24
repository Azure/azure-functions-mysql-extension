/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package com.microsoft.azure.functions.mysql.annotation;

import java.lang.annotation.Retention;
import java.lang.annotation.Target;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.ElementType;

import com.microsoft.azure.functions.annotation.CustomBinding;

@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
@CustomBinding(direction = "in", name = "", type = "mysqlTrigger")
public @interface MySqlTrigger {
    /**
     * The variable name used in function.json.
    */
    String name();

    /**
     * Name of the table to watch for changes.
    */
    String tableName();

    /**
     * Setting name for MySql connection string.
    */
    String connectionStringSetting();

    /**
     * Optional. Name of the table used to store leases. If not specified, the leases table name will be
     * Leases_{FunctionId}_{TableId}.
     */
    String leasesTableName() default "";
}