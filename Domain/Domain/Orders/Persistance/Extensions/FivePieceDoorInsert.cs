using Dapper;
using Domain.Orders.Entities.Products.Doors;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(FivePieceDoorProduct door, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertFivePieceDoorConfig(door.Id, door, connection, trx);
            await InsertIntoProductTable(door, orderId, connection, trx);
            await connection.ExecuteAsync(
                """
                INSERT INTO five_piece_door_products
                    (product_id,
                    width,
                    height,
                    top_rail,
                    bottom_rail,
                    left_stile,
                    right_stile)
                VALUES
                    (@ProductId,
                    @Width,
                    @Height,
                    @TopRail,
                    @BottomRail,
                    @LeftStile,
                    @RightStile);
                """,
                new {
                    ProductId = door.Id,
                    door.Width,
                    door.Height,
                    door.FrameSize.TopRail,
                    door.FrameSize.BottomRail,
                    door.FrameSize.LeftStile,
                    door.FrameSize.RightStile,
                },
                trx);

        }

    }
}
