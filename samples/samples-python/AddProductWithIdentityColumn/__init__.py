# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import azure.functions as func
from Common.productWithoutId import ProductWithoutId

def main(req: func.HttpRequest, product: func.Out[func.MySqlRow]) -> func.HttpResponse:
    """This shows an example of a MySql Output binding where the target table has a primary key
    which is an identity column. In such a case the primary key is not required to be in
    the object used by the binding - it will insert a row with the other values and the
    ID will be generated upon insert.
    """

    row_obj = func.MySqlRow(ProductWithoutId(req.params["name"],req.params["cost"]))
    product.set(row_obj)

    return func.HttpResponse(
        body=row_obj.to_json(),
        status_code=201,
        mimetype="application/json"
    )
