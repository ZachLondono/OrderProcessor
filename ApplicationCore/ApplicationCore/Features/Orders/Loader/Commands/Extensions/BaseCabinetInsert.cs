using ApplicationCore.Features.Orders.Shared.Domain.Products;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(BaseCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            Guid? mdfConfigId = null;
            var mdfConfig = cabinet.MDFDoorOptions;
            if (mdfConfig is not null) {
                mdfConfigId = Guid.NewGuid();
                await InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            Guid? rollOutConfigId = null;
            if (cabinet.Inside.RollOutBoxes.Any()) {
                rollOutConfigId = Guid.NewGuid();
                await InsertRollOutConfig((Guid)rollOutConfigId, cabinet.Inside.RollOutBoxes, connection, trx);
            }

            var dbConfigId = Guid.NewGuid();
            await InsertDBConfig(dbConfigId, cabinet.DrawerBoxOptions, connection, trx);

            await InsertIntoProductTable(cabinet, orderId, connection, trx);
            await InsertCabinet(cabinet, mdfConfigId, connection, trx);

            var parameters = new {
                ProductId = cabinet.Id,
                ToeType = cabinet.ToeType,
                DoorQty = cabinet.Doors.Quantity,
                HingeSide = cabinet.Doors.HingeSide,
                ROConfigId = rollOutConfigId,
                AdjShelfQty = cabinet.Inside.AdjustableShelves,
                VertDivQty = cabinet.Inside.VerticalDividers,
                ShelfDepth = cabinet.Inside.ShelfDepth,
                DrawerFaceHeight = cabinet.Drawers.FaceHeight,
                DrawerQty = cabinet.Drawers.Quantity,
                DBConfigId = dbConfigId
            };

            await connection.ExecuteAsync("""
                    INSERT INTO base_cabinets
                        (product_id,
                        toe_type,
                        door_qty,
                        hinge_side,
                        roll_out_config_id,
                        adj_shelf_qty,
                        vert_div_qty,
                        shelf_depth,
                        drawer_face_height,
                        drawer_qty,
                        db_config_id)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @DoorQty,
                        @HingeSide,
                        @ROConfigId,
                        @AdjShelfQty,
                        @VertDivQty,
                        @ShelfDepth,
                        @DrawerFaceHeight,
                        @DrawerQty,
                        @DBConfigId);
                    """, parameters, trx);

        }


    }

}
