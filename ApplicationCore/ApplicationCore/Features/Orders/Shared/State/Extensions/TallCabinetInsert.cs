using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Shared.State;

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
                DBConfigId = dbConfigId
            };

            await connection.ExecuteAsync("""
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
                        db_config_id)
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
                        @DBConfigId);
                    """, parameters, trx);

        }


    }

}
