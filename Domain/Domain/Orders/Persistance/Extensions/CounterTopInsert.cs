using Domain.Orders.Entities.Products;
using System.Data;
using Dapper;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(CounterTop counter, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertIntoProductTable(counter, orderId, connection, trx);

            var parameters = new {
                ProductId = counter.Id,
                Finish = counter.Finish,
                Width = counter.Width,
                Length = counter.Length,
                EdgeBanding = counter.EdgeBanding
            };

            await connection.ExecuteAsync(
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
