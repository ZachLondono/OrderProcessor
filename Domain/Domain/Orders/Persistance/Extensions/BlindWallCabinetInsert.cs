using Domain.Orders.Entities.Products.Cabinets;
using Dapper;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(BlindWallCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

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
                BlindSide = cabinet.BlindSide,
                BlindWidth = cabinet.BlindWidth,
                AdjShelfQty = cabinet.AdjustableShelves,
                DoorExtendDown = cabinet.ExtendedDoor,
                DoorQty = cabinet.Doors.Quantity,
                HingeSide = cabinet.Doors.HingeSide,
                IsGarage = cabinet.IsGarage
            };

            await connection.ExecuteAsync("""
                    INSERT INTO blind_wall_cabinets
                        (product_id,
                        blind_side,
                        blind_width,
                        adj_shelf_qty,
                        door_extend_down,
                        door_qty,
                        hinge_side,
                        is_garage)
                    VALUES
                        (@ProductId,
                        @BlindSide,
                        @BlindWidth,
                        @AdjShelfQty,
                        @DoorExtendDown,
                        @DoorQty,
                        @HingeSide,
                        @IsGarage);
                    """, parameters, trx);

        }

    }

}
