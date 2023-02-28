using ApplicationCore.Features.Orders.Shared.Domain.Products;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(SinkCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            Guid? mdfConfigId = null;
            var mdfConfig = cabinet.MDFDoorOptions;
            if (mdfConfig is not null) {
                mdfConfigId = Guid.NewGuid();
                await InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            Guid rollOutConfigId = Guid.NewGuid();
            await InsertRollOutConfig(rollOutConfigId, cabinet.RollOutBoxes, connection, trx);

            var dbConfigId = Guid.NewGuid();
            await InsertCabinetDBConfig(dbConfigId, cabinet.DrawerBoxOptions, connection, trx);

            await InsertIntoProductTable(cabinet, orderId, connection, trx);
            await InsertCabinet(cabinet, mdfConfigId, connection, trx);

            var parameters = new {
                ProductId = cabinet.Id,
                ToeType = cabinet.ToeType,
                HingeSide = cabinet.HingeSide,
                DoorQty = cabinet.DoorQty,
                FalseDrawerQty = cabinet.FalseDrawerQty,
                DrawerFaceHeight = cabinet.DrawerFaceHeight,
                AdjShelfQty = cabinet.AdjustableShelves,
                ShelfDepth = cabinet.ShelfDepth,
                RollOutConfigId = rollOutConfigId,
                DBConfigId = dbConfigId
            };

            await connection.ExecuteAsync("""
                    INSERT INTO sink_cabinets
                        (product_id,
                        toe_type,
                        hinge_side,
                        door_qty,
                        false_drawer_qty,
                        drawer_face_height,
                        adj_shelf_qty,
                        shelf_depth,
                        roll_out_config_id,
                        db_config_id)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @HingeSide,
                        @DoorQty,
                        @FalseDrawerQty,
                        @DrawerFaceHeight,
                        @AdjShelfQty,
                        @ShelfDepth,
                        @RollOutConfigId,
                        @DBConfigId);
                    """, parameters, trx);

        }


    }

}
