using Domain.Orders.Entities.Products.Cabinets;
using Domain.ValueObjects;
using Dapper;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(BaseCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

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
                DoorQty = cabinet.Doors.Quantity,
                HingeSide = cabinet.Doors.HingeSide,
                RolloutConfigPositions = cabinet.Inside.RollOutBoxes.Positions,
                RolloutConfigBlockType = cabinet.Inside.RollOutBoxes.Blocks,
                RolloutConfigScoopFront = cabinet.Inside.RollOutBoxes.ScoopFront,
                AdjShelfQty = cabinet.Inside.AdjustableShelves,
                VertDivQty = cabinet.Inside.VerticalDividers,
                ShelfDepth = cabinet.Inside.ShelfDepth,
                DrawerFaceHeight = cabinet.Drawers.FaceHeight,
                DrawerQty = cabinet.Drawers.Quantity,
                DBConfigId = dbConfigId,
                IsGarage = cabinet.IsGarage,
                BaseNotchHeight = cabinet.BaseNotch?.Height ?? Dimension.Zero,
                BaseNotchDepth = cabinet.BaseNotch?.Depth ?? Dimension.Zero
            };

            await connection.ExecuteAsync("""
                    INSERT INTO base_cabinets
                        (product_id,
                        toe_type,
                        door_qty,
                        hinge_side,
                        rollout_positions,
                        rollout_block_type,
                        rollout_scoop_front,
                        adj_shelf_qty,
                        vert_div_qty,
                        shelf_depth,
                        drawer_face_height,
                        drawer_qty,
                        db_config_id,
                        is_garage,
                        base_notch_height,
                        base_notch_depth)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @DoorQty,
                        @HingeSide,
                        @RolloutConfigPositions,
                        @RolloutConfigBlockType,
                        @RolloutConfigScoopFront,
                        @AdjShelfQty,
                        @VertDivQty,
                        @ShelfDepth,
                        @DrawerFaceHeight,
                        @DrawerQty,
                        @DBConfigId,
                        @IsGarage,
                        @BaseNotchHeight,
                        @BaseNotchDepth);
                    """, parameters, trx);

        }


    }

}
