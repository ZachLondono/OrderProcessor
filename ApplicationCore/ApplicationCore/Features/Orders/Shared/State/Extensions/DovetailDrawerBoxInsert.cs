using System.Data;
using Dapper;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(DovetailDrawerBoxProduct drawerBox, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertDovetailDBConfig(drawerBox.Id, drawerBox.DrawerBoxOptions, connection, trx);
            await InsertIntoProductTable(drawerBox, orderId, connection, trx);

            var parameters = new {
                ProductId = drawerBox.Id,
                drawerBox.Height,
                drawerBox.Width,
                drawerBox.Depth,
                drawerBox.Note,
                LabelFields = (IDictionary<string, string>)drawerBox.LabelFields
            };

            await connection.ExecuteAsync("""
                    INSERT INTO dovetail_drawer_products
                        (product_id,
                        height,
                        width,
                        depth,
                        note,
                        label_fields)
                    VALUES
                        (@ProductId,
                        @Height,
                        @Width,
                        @Depth,
                        @Note,
                        @LabelFields);
                    """, parameters, trx);

        }

    }

}