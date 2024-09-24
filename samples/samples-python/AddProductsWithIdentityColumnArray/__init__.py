# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import json
import azure.functions as func
from Common.productWithoutId import ProductWithoutId

def main(req: func.HttpRequest, products: func.Out[func.MySqlRow]) -> func.HttpResponse:
    """This shows an example of a MySql Output binding where the target table has a primary key
    which is an identity column. In such a case the primary key is not required to be in
    the object used by the binding - it will insert a row with the other values and the
    ID will be generated upon insert.
    """

    row_objs = [func.MySqlRow(ProductWithoutId("Cup", 2)),
               func.MySqlRow(ProductWithoutId("Glasses", 12))]
    products.set(row_objs)

    return func.HttpResponse(
        body=json.dumps(list(map(lambda r: json.loads(r.to_json()), row_objs))),
        status_code=201,
        mimetype="application/json"
    )
