using Domain.Orders.Entities.Products.Cabinets;
using Domain.ValueObjects;
using Dapper;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(DrawerBaseCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

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
                FaceHeights = cabinet.Drawers.FaceHeights,
                DBConfigId = dbConfigId,
                IsGarage = cabinet.IsGarage,
                BaseNotchHeight = cabinet.BaseNotch?.Height ?? Dimension.Zero,
                BaseNotchDepth = cabinet.BaseNotch?.Depth ?? Dimension.Zero
            };

            await connection.ExecuteAsync("""
                    INSERT INTO drawer_base_cabinets
                        (product_id,
                        toe_type,
                        face_heights,
                        db_config_id,
                        is_garage,
                        base_notch_height,
                        base_notch_depth)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @FaceHeights,
                        @DBConfigId,
                        @IsGarage,
                        @BaseNotchHeight,
                        @BaseNotchDepth);
                    """, parameters, trx);

        }

    }
}
