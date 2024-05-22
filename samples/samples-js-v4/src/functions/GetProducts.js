const { app, input } = require('@azure/functions');

// The input binding executes the `select * from Products where Cost = @Cost` query, returning the result as json object in the body.
// The *parameters* argument passes the `{cost}` specified in the URL that triggers the function,
// `getproducts/{cost}`, as the value of the `@Cost` parameter in the query.
// *commandType* is set to `Text`, since the constructor argument of the binding is a raw query.
const mysqlInput = input.generic({
    type: 'mysql',
    commandText: 'select * from Products where Cost = @Cost',
    commandType: 'Text',
    parameters: '@Cost={cost}',
    connectionStringSetting: 'MySqlConnectionString'
})

app.http('GetProducts', {
    methods: ['GET', 'POST'],
    authLevel: 'anonymous',
    route: 'getproducts/{cost}',
    extraInputs: [mysqlInput],
    handler: async (request, context) => {
        const products = JSON.stringify(context.extraInputs.get(mysqlInput));

        return {
            status: 200,
            body: products
        };
    }
});
