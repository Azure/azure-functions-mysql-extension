import json
import logging
import azure.functions as func
from azure.functions.decorators.core import DataType

app = func.FunctionApp()

@app.function_name(name="GetProducts")
@app.route(route="getproducts/{cost}")
@app.generic_input_binding(arg_name="product",                           
                        type="mysql",
                        command_text="select * from Products where Cost = @Cost",
                        command_type="Text",
                        parameters="@Cost={cost}",
                        connection_string_setting="MySqlConnectionString")

def getproducts(req: func.HttpRequest, product: func.MySqlRowList) -> func.HttpResponse:
    rows = list(map(lambda r: json.loads(r.to_json()), product))

    return func.HttpResponse(
        json.dumps(rows),
        status_code=200,
        mimetype="application/json"
    )

@app.function_name(name="AddProduct")
@app.route(route="addproduct")
@app.generic_output_binding(arg_name="product",
                           type="mysql",
                           commandText= "Products",
                           command_type="Text",
                           connection_string_setting="MySqlConnectionString")
 
def add_product(req: func.HttpRequest, product: func.Out[func.MySqlRow]) -> func.HttpResponse:
    body = json.loads(req.get_body())
    row = func.MySqlRow.from_dict(body)
    product.set(row)
 
    return func.HttpResponse(
        body=req.get_body(),
        status_code=201,
        mimetype="application/json"
    )


# The function gets triggered when a change (Insert, Update)
# is made to the Products table.
@app.function_name(name="ProductsTrigger")
@app.generic_trigger(arg_name="products",
                        type="mysqlTrigger",
                        table_name="Products",
                        connection_string_setting="MySqlConnectionString")
 
def products_trigger(products: str) -> None:
    logging.info("MySQL Changes: %s", json.loads(products))

