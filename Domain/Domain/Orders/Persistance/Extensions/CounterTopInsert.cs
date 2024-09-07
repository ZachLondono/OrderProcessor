using Domain.Orders.Entities.Products;
using System.Data;
using Dapper;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(CounterTop counter, Guid orderId, IDbConnection connection, IDbTransaction trx) {

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
