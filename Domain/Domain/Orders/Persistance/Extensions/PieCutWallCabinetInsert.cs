using Domain.Orders.Entities.Products.Cabinets;
using Dapper;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(WallPieCutCornerCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

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
                DoorExtendDown = cabinet.ExtendedDoor,
                AdjShelfQty = cabinet.AdjustableShelves
            };

            connection.Execute("""
                    INSERT INTO pie_cut_wall_cabinets
                        (product_id,
                        right_width,
                        right_depth,
                        hinge_side,
                        door_extend_down,
                        adj_shelf_qty)
                    VALUES
                        (@ProductId,
                        @RightWidth,
                        @RightDepth,
                        @HingeSide,
                        @DoorExtendDown,
                        @AdjShelfQty);
                    """, parameters, trx);

        }

    }

}