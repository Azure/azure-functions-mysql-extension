/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package com.function;

import com.function.Common.Product;
import com.microsoft.azure.functions.HttpMethod;
import com.microsoft.azure.functions.HttpRequestMessage;
import com.microsoft.azure.functions.HttpResponseMessage;
import com.microsoft.azure.functions.HttpStatus;
import com.microsoft.azure.functions.OutputBinding;
import com.microsoft.azure.functions.annotation.AuthorizationLevel;
import com.microsoft.azure.functions.annotation.FunctionName;
import com.microsoft.azure.functions.annotation.HttpTrigger;
import com.microsoft.azure.functions.mysql.annotation.MySqlInput;
import com.microsoft.azure.functions.mysql.annotation.CommandType;
import com.microsoft.azure.functions.mysql.annotation.MySqlOutput;

import java.util.Optional;

/**
 * This function uses a MySQL input binding to get products from the Products table
 * and upsert those products to the ProductsWithIdentity table.
 */
public class GetAndAddProducts {
    @FunctionName("GetAndAddProducts")
    public HttpResponseMessage run(
            @HttpTrigger(
                name = "req",
                methods = {HttpMethod.GET},
                authLevel = AuthorizationLevel.ANONYMOUS,
                route = "getandaddproducts/{cost}")
                HttpRequestMessage<Optional<String>> request,
            @MySqlInput(
                name = "products",
                commandText = "SELECT * FROM Products WHERE Cost = @Cost",
                commandType = CommandType.Text,
                parameters = "@Cost={cost}",
                connectionStringSetting = "MySqlConnectionString")
                Product[] products,
            @MySqlOutput(
                name = "productsWithIdentity",
                commandText = "ProductsWithIdentity",
                connectionStringSetting = "MySqlConnectionString")
                OutputBinding<Product[]> productsWithIdentity) {

        productsWithIdentity.setValue(products);

        return request.createResponseBuilder(HttpStatus.OK).header("Content-Type", "application/json").body(products).build();
    }
}
