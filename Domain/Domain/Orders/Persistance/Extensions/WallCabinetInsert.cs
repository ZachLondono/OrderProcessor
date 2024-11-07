using Domain.Infrastructure.Data;
using Domain.Orders.Entities.Products.Cabinets;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(WallCabinet cabinet, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            Guid? mdfConfigId = null;
            if (cabinet.DoorConfiguration.TryGetMDFOptions(out var mdfConfig)) {
                mdfConfigId = Guid.NewGuid();
                InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            InsertIntoProductTable(cabinet, orderId, connection, trx);
            InsertCabinet(cabinet, mdfConfigId, connection, trx);

            var parameters = new {
                ProductId = cabinet.Id,
                DoorQty = cabinet.Doors.Quantity,
                HingeSide = cabinet.Doors.HingeSide,
                DoorExtendDown = cabinet.Doors.ExtendDown,
                AdjShelfQty = cabinet.Inside.AdjustableShelves,
                VertDivQty = cabinet.Inside.VerticalDividers,
                FinishedBottom = cabinet.FinishedBottom,
                IsGarage = cabinet.IsGarage
            };

            connection.Execute("""
                    INSERT INTO wall_cabinets
                        (product_id,
                        door_qty,
                        hinge_side,
                        door_extend_down,
                        adj_shelf_qty,
                        vert_div_qty,
                        finished_bottom,
                        is_garage)
                    VALUES
                        (@ProductId,
                        @DoorQty,
                        @HingeSide,
                        @DoorExtendDown,
                        @AdjShelfQty,
                        @VertDivQty,
                        @FinishedBottom,
                        @IsGarage);
                    """, parameters, trx);

        }

    }
}