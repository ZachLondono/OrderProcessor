using System.Data;
using Dapper;
using Domain.Orders.Entities.Products.DrawerBoxes;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(DoweledDrawerBoxProduct drawerBox, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            InsertDoweledDBConfig(drawerBox.Id, drawerBox, connection, trx);
            InsertIntoProductTable(drawerBox, orderId, connection, trx);

            var parameters = new {
                ProductId = drawerBox.Id,
                Height = drawerBox.Height,
                Width = drawerBox.Width,
                Depth = drawerBox.Depth
            };

            connection.Execute(
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
