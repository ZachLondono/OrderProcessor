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
                drawerbox.Depth,
                drawerbox.Note,
                LabelFields = (IDictionary<string, string>)drawerbox.LabelFields,
                drawerbox.Room
            };

            await connection.ExecuteAsync("""
                    INSERT INTO dovetail_drawer_products
                        (product_id,
                        height,
                        width,
                        depth,
                        note,
                        label_fields,
                        room)
                    VALUES
                        (@ProductId,
                        @Height,
                        @Width,
                        @Depth,
                        @Note,
                        @LabelFields,
                        @Room);
                    """, parameters, trx);

        }

    }

}