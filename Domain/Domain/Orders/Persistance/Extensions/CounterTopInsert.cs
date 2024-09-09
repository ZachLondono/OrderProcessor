using Domain.Orders.Entities.Products;
using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(CounterTop counter, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            InsertIntoProductTable(counter, orderId, connection, trx);

            var parameters = new {
                ProductId = counter.Id,
                Finish = counter.Finish,
                Width = counter.Width,
                Length = counter.Length,
                EdgeBanding = counter.EdgeBanding
            };

            connection.Execute(
                """
                INSERT INTO counter_tops 
                    (product_id,
                    finish,
                    width,
                    length,
                    edge_banding)
                VALUES
                    (@ProductId,
                    @Finish,
                    @Width,
                    @Length,
                    @EdgeBanding);
                """, parameters, trx);

        }


    }
}
