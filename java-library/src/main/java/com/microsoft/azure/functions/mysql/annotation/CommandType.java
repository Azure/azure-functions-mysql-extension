/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */
package com.microsoft.azure.functions.mysql.annotation;

/**
 * The type of commandText.
 */
public enum CommandType {
    /**
     * The commandText is a MySql query.
     */
    Text,

    /**
     * The commandText is a MySql stored procedure.
     */
    StoredProcedure
}
