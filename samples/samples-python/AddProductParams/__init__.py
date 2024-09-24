# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

from urllib.parse import parse_qs, urlparse

import azure.functions as func
from Common.product import Product

def main(req: func.HttpRequest, product: func.Out[func.MySqlRow]) -> func.HttpResponse:
    # The Python worker discards empty query parameters so use parse_qs as a workaround.
    params = parse_qs(urlparse(req.url).query, keep_blank_values=True)
    row = func.MySqlRow(Product(params["ProductId"][0], params["Name"][0], params["Cost"][0]))
    product.set(row)

    return func.HttpResponse(
        body=row.to_json(),
        status_code=201,
        mimetype="application/json"
    )
