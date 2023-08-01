using System.Data;
using Dapper;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(WallCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            Guid? mdfConfigId = null;
            var mdfConfig = cabinet.MDFDoorOptions;
            if (mdfConfig is not null) {
                mdfConfigId = Guid.NewGuid();
                await InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            await InsertIntoProductTable(cabinet, orderId, connection, trx);
            await InsertCabinet(cabinet, mdfConfigId, connection, trx);

            var parameters = new {
                ProductId = cabinet.Id,
                DoorQty = cabinet.Doors.Quantity,
                HingeSide = cabinet.Doors.HingeSide,
                DoorExtendDown = cabinet.Doors.ExtendDown,
                AdjShelfQty = cabinet.Inside.AdjustableShelves,
                VertDivQty = cabinet.Inside.VerticalDividers,
                FinishedBottom = cabinet.FinishedBottom
            };

            await connection.ExecuteAsync("""
                    INSERT INTO wall_cabinets
                        (product_id,
                        door_qty,
                        hinge_side,
                        door_extend_down,
                        adj_shelf_qty,
                        vert_div_qty,
                        finished_bottom)
                    VALUES
                        (@ProductId,
                        @DoorQty,
                        @HingeSide,
                        @DoorExtendDown,
                        @AdjShelfQty,
                        @VertDivQty,
                        @FinishedBottom);
                    """, parameters, trx);

        }

    }
}