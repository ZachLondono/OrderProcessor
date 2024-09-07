using Dapper;
using Domain.Orders.Entities.Products.Doors;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(FivePieceDoorProduct door, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            InsertFivePieceDoorConfig(door.Id, door, connection, trx);
            InsertIntoProductTable(door, orderId, connection, trx);
            connection.Execute(
                """
                INSERT INTO five_piece_door_products
                    (product_id,
                    width,
                    height,
                    top_rail,
                    bottom_rail,
                    left_stile,
                    right_stile,
                    type)
                VALUES
                    (@ProductId,
                    @Width,
                    @Height,
                    @TopRail,
                    @BottomRail,
                    @LeftStile,
                    @RightStile,
                    @DoorType);
                """,
                new {
                    ProductId = door.Id,
                    door.Width,
                    door.Height,
                    door.FrameSize.TopRail,
                    door.FrameSize.BottomRail,
                    door.FrameSize.LeftStile,
                    door.FrameSize.RightStile,
                    door.DoorType
                },
                trx);

        }

    }
}
