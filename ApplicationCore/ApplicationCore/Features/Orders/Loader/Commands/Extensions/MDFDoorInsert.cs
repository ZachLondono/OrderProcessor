using ApplicationCore.Features.Orders.Shared.Domain.Products;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(MDFDoorProduct mdfdoor, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertMDFConfig(mdfdoor.Id, mdfdoor, connection, trx);
            await InsertIntoProductTable(mdfdoor, orderId, connection, trx);

            var parameters = new {
                ProductId = mdfdoor.Id,
                mdfdoor.Room,
                mdfdoor.Note,
                mdfdoor.Height,
                mdfdoor.Width,
                mdfdoor.Type,
                mdfdoor.FrameSize.TopRail,
                mdfdoor.FrameSize.BottomRail,
                mdfdoor.FrameSize.LeftStile,
                mdfdoor.FrameSize.RightStile,
            };

            await connection.ExecuteAsync("""
                    INSERT INTO mdf_door_products
                        (product_id,
                        room,
                        note,
                        height,
                        width,
                        type,
                        top_rail,
                        bottom_rail,
                        left_stile,
                        right_stile)
                    VALUES
                        (@ProductId,
                        @Room,
                        @Note,
                        @Height,
                        @Width,
                        @Type,
                        @TopRail,
                        @BottomRail,
                        @LeftStile,
                        @RightStile);
                    """, parameters, trx);

        }

    }

}