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
@Target({ ElementType.PARAMETER, ElementType.METHOD })
@CustomBinding(direction = "out", name = "", type = "mysql")
public @interface MySqlOutput {
    /**
     * The variable name used in function.json.
     */
    String name();

    /**
     * Name of the table to upsert data to.
     */
    String commandText() default "";

    /**
     * Setting name for MySql connection string.
     */
    String connectionStringSetting() default "";
}
