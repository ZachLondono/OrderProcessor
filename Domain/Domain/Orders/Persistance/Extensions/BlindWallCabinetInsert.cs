using Domain.Orders.Entities.Products.Cabinets;
using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(BlindWallCabinet cabinet, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            Guid? mdfConfigId = null;
            if (cabinet.DoorConfiguration.TryGetMDFOptions(out var mdfConfig)) {
                mdfConfigId = Guid.NewGuid();
                InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            InsertIntoProductTable(cabinet, orderId, connection, trx);
            InsertCabinet(cabinet, mdfConfigId, connection, trx);

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

            connection.Execute("""
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
