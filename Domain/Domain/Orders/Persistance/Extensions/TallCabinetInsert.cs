using Domain.Orders.Entities.Products.Cabinets;
using Domain.ValueObjects;
using Dapper;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(TallCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

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
                LowerAdjShelfQty = cabinet.Inside.AdjustableShelvesLower,
                UpperAdjShelfQty = cabinet.Inside.AdjustableShelvesUpper,
                LowerVertDivQty = cabinet.Inside.VerticalDividersLower,
                UpperVertDivQty = cabinet.Inside.VerticalDividersUpper,
                RolloutConfigPositions = cabinet.Inside.RollOutBoxes.Positions,
                RolloutConfigBlockType = cabinet.Inside.RollOutBoxes.Blocks,
                RolloutConfigScoopFront = cabinet.Inside.RollOutBoxes.ScoopFront,
                LowerDoorQty = cabinet.Doors.LowerQuantity,
                UpperDoorQty = cabinet.Doors.UpperQuantity,
                LowerDoorHeight = cabinet.Doors.LowerDoorHeight,
                HingeSide = cabinet.Doors.HingeSide,
                DBConfigId = dbConfigId,
                IsGarage = cabinet.IsGarage,
                BaseNotchHeight = cabinet.BaseNotch?.Height ?? Dimension.Zero,
                BaseNotchDepth = cabinet.BaseNotch?.Depth ?? Dimension.Zero
            };

            connection.Execute("""
                    INSERT INTO tall_cabinets
                        (product_id,
                        toe_type,
                        lower_adj_shelf_qty,
                        upper_adj_shelf_qty,
                        lower_vert_div_qty,
                        upper_vert_div_qty,
                        rollout_positions,
                        rollout_block_type,
                        rollout_scoop_front,
                        lower_door_qty,
                        upper_door_qty,
                        lower_door_height,
                        hinge_side,
                        db_config_id,
                        is_garage,
                        base_notch_height,
                        base_notch_depth)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @LowerAdjShelfQty,
                        @UpperAdjShelfQty,
                        @LowerVertDivQty,
                        @UpperVertDivQty,
                        @RolloutConfigPositions,
                        @RolloutConfigBlockType,
                        @RolloutConfigScoopFront,
                        @LowerDoorQty,
                        @UpperDoorQty,
                        @LowerDoorHeight,
                        @HingeSide,
                        @DBConfigId,
                        @IsGarage,
                        @BaseNotchHeight,
                        @BaseNotchDepth);
                    """, parameters, trx);

        }


    }

}
