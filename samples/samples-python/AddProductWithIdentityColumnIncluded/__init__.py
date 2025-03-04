# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import azure.functions as func
from Common.product import Product

def main(req: func.HttpRequest, product: func.Out[func.MySqlRow]) -> func.HttpResponse:
    """This shows an example of a MySql Output binding where the target table has a primary key
    which is an identity column and the identity column is included in the output object. In
    this case the identity column is used to match the rows and all non-identity columns are
    updated if a match is found. Otherwise a new row is inserted (with the identity column being
    ignored since that will be generated by the engine).
    """

    row_obj = func.MySqlRow(Product(req.params["productId"] if "productId" in req.params else None, req.params["name"],req.params["cost"]))
    product.set(row_obj)

    return func.HttpResponse(
        body=row_obj.to_json(),
        status_code=201,
        mimetype="application/json"
    )
