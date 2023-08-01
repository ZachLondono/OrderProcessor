using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(FivePieceDoorProduct door, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertFivePieceDoor(door.Id, door, connection, trx);
            await InsertIntoProductTable(door, orderId, connection, trx);
            await connection.ExecuteAsync(
                "INSERT INTO five_piece_door_products (product_id) VALUES (@ProductId);",
                new {
                    ProductId = door.Id,
                },
                trx);

        }

    }
}
