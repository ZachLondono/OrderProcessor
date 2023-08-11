using System.Data;
using Dapper;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(DoweledDrawerBoxProduct drawerBox, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertDoweledDBConfig(drawerBox.Id, drawerBox, connection, trx);
            await InsertIntoProductTable(drawerBox, orderId, connection, trx);

            var parameters = new {
                ProductId = drawerBox.Id,
                Height = drawerBox.Height,
                Width = drawerBox.Width,
                Depth = drawerBox.Depth
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO doweled_drawer_products 
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
