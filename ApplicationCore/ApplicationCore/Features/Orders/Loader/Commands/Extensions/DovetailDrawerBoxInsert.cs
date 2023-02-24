using ApplicationCore.Features.Orders.Shared.Domain.Products;
using System.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(DovetailDrawerBoxProduct drawerbox, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertDBConfig(drawerbox.Id, drawerbox.DrawerBoxOptions, connection, trx);
            await InsertIntoProductTable(drawerbox, orderId, connection, trx);

            var parameters = new {
                ProductId = drawerbox.Id,
                drawerbox.Height,
                drawerbox.Width,
                drawerbox.Depth
            };

            await connection.ExecuteAsync("""
                    INSERT INTO dovetail_door_products
                        (product_id,
                        height,
                        width,
                        depth)
                    VALUES
                        (@ProductId,
                        @Height,
                        @Width,
                        @Depth);
                    """, parameters, trx);

        }

    }

}