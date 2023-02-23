using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(TallCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

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
                LowerAdjShelfQty = cabinet.Inside.AdjustableShelvesLower,
                UpperAdjShelfQty = cabinet.Inside.AdjustableShelvesUpper,
                LowerVertDivQty = cabinet.Inside.VerticalDividersLower,
                UpperVertDivQty = cabinet.Inside.VerticalDividersUpper,
                RollOutConfigId = rollOutConfigId,
                LowerDoorQty = cabinet.Doors.LowerDoorHeight,
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
                        roll_out_config,
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
                        @RollOutConfigId,
                        @LowerDoorQty,
                        @UpperDoorQty,
                        @LowerDoorHeight,
                        @HingeSide,
                        @DBConfigId);
                    """, parameters, trx);

        }


    }

}
