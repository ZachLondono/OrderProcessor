using ApplicationCore.Features.Orders.Shared.Domain.Products;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(TrashCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

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
                TrashConfig = cabinet.TrashPulloutConfiguration,
                DrawerFaceHeight = cabinet.DrawerFaceHeight,
                DBConfigId = dbConfigId
            };

            await connection.ExecuteAsync("""
                    INSERT INTO trash_cabinets
                        (product_id,
                        toe_type,
                        trash_config,
                        drawer_face_height,
                        db_config_id)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @TrashConfig,
                        @DrawerFaceHeight,
                        @DBConfigId);
                    """, parameters, trx);

        }


    }

}