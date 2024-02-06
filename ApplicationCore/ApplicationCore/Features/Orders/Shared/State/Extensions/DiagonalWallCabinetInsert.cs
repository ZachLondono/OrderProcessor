using Domain.Orders.Products.Cabinets;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(WallDiagonalCornerCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

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
                RightWidth = cabinet.RightWidth,
                RightDepth = cabinet.RightDepth,
                HingeSide = cabinet.HingeSide,
                DoorQty = cabinet.DoorQty,
                DoorExtendDown = cabinet.ExtendedDoor,
                AdjShelfQty = cabinet.AdjustableShelves,
                IsGarage = cabinet.IsGarage
            };

            await connection.ExecuteAsync("""
                    INSERT INTO diagonal_wall_cabinets
                        (product_id,
                        right_width,
                        right_depth,
                        hinge_side,
                        door_qty,
                        door_extend_down,
                        adj_shelf_qty,
                        is_garage)
                    VALUES
                        (@ProductId,
                        @RightWidth,
                        @RightDepth,
                        @HingeSide,
                        @DoorQty,
                        @DoorExtendDown,
                        @AdjShelfQty,
                        @IsGarage);
                    """, parameters, trx);

        }

    }

}
