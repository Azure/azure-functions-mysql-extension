# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import azure.functions as func
from Common.product import Product

# This output binding should throw an error since the ProductsNameNotNull table does not
# allows rows without a Name value. No rows should be upserted to the MySql table.
def main(req: func.HttpRequest, products: func.Out[func.MySqlRowList]) -> func.HttpResponse:
    rows = func.MySqlRowList()
    for i in range(1000):
        rows.add(func.MySqlRow(Product(i, "test", 100 * i)))

    invalidProduct = func.MySqlRow(Product(1, None, 100))

    rows.add(invalidProduct)

    products.set(rows)

    return func.HttpResponse(
        rows.to_json(),
        status_code=201,
        mimetype="application/json"
    )
