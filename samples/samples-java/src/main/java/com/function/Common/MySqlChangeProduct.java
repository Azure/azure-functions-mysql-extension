/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package com.function.Common;

public class MySqlChangeProduct {
    private MySqlChangeOperation Operation;
    private Product Item;

    public MySqlChangeProduct() {
    }

    public MySqlChangeProduct(MySqlChangeOperation operation, Product item) {
        this.Operation = operation;
        this.Item = item;
    }

    public MySqlChangeOperation getOperation() {
        return Operation;
    }

    public void setOperation(MySqlChangeOperation operation) {
        this.Operation = operation;
    }

    public Product getItem() {
        return Item;
    }

    public void setItem(Product item) {
        this.Item = item;
    }
}