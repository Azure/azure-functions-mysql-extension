const { app, output } = require('@azure/functions');

const mysqlOutput = output.generic({
    type: 'mysql',
    commandText: 'Products',
    connectionStringSetting: 'MySqlConnectionString'
})

// Upsert the product, which will insert it into the Products table if the primary key (ProductId) for that item doesn't exist.
// If it does then update it to have the new name and cost.
app.http('AddProduct', {
    methods: ['POST'],
    authLevel: 'anonymous',
    extraOutputs: [mysqlOutput],
    handler: async (request, context) => {
        // Note that this expects the body to be a JSON object or array of objects which have a property
        // matching each of the columns in the table to upsert to.
        const product = await request.json();
        context.extraOutputs.set(mysqlOutput, product);

        return {
            status: 201,
            body: JSON.stringify(product)
        };
    }
});
