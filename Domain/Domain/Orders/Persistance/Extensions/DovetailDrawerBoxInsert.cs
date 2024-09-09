using Domain.Infrastructure.Data;
using Domain.Orders.Entities.Products.DrawerBoxes;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(DovetailDrawerBoxProduct drawerBox, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            InsertDovetailDBConfig(drawerBox.Id, drawerBox.DrawerBoxOptions, connection, trx);
            InsertIntoProductTable(drawerBox, orderId, connection, trx);

            var parameters = new {
                ProductId = drawerBox.Id,
                drawerBox.Height,
                drawerBox.Width,
                drawerBox.Depth,
                drawerBox.Note,
                LabelFields = (IDictionary<string, string>)drawerBox.LabelFields
            };

            connection.Execute("""
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