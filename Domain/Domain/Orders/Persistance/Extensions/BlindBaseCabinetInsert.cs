using Domain.Orders.Entities.Products.Cabinets;
using Dapper;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(BlindBaseCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            Guid? mdfConfigId = null;
            var mdfConfig = cabinet.MDFDoorOptions;
            if (mdfConfig is not null) {
                mdfConfigId = Guid.NewGuid();
                await InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            var dbConfigId = Guid.NewGuid();
            await InsertCabinetDBConfig(dbConfigId, cabinet.DrawerBoxOptions, connection, trx);

            await InsertIntoProductTable(cabinet, orderId, connection, trx);
            await InsertCabinet(cabinet, mdfConfigId, connection, trx);

            var parameters = new {
                ProductId = cabinet.Id,
                ToeType = cabinet.ToeType,
                AdjShelfQty = cabinet.AdjustableShelves,
                ShelfDepth = cabinet.ShelfDepth,
                BlindSide = cabinet.BlindSide,
                BlindWidth = cabinet.BlindWidth,
                DoorQty = cabinet.Doors.Quantity,
                HingeSide = cabinet.Doors.HingeSide,
                DrawerQty = cabinet.Drawers.Quantity,
                DrawerFaceHeight = cabinet.Drawers.FaceHeight,
                DBConfigId = dbConfigId
            };

            await connection.ExecuteAsync("""
                    INSERT INTO blind_base_cabinets
                        (product_id,
                        toe_type,
                        adj_shelf_qty,
                        shelf_depth,
                        blind_side,
                        blind_width,
                        door_qty,
                        hinge_side,
                        drawer_qty,
                        drawer_face_height,
                        db_config_id)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @AdjShelfQty,
                        @ShelfDepth,
                        @BlindSide,
                        @BlindWidth,
                        @DoorQty,
                        @HingeSide,
                        @DrawerQty,
                        @DrawerFaceHeight,
                        @DBConfigId);
                    """, parameters, trx);

        }

    }

}
