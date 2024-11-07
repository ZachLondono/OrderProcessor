using Domain.Orders.Entities.Products.Cabinets;
using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(TrashCabinet cabinet, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            Guid? mdfConfigId = null;
            if (cabinet.DoorConfiguration.TryGetMDFOptions(out var mdfConfig)) {
                mdfConfigId = Guid.NewGuid();
                InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            Guid? dbConfigId = null;
            if (cabinet.DrawerBoxOptions is not null) {
                dbConfigId = Guid.NewGuid();
                InsertCabinetDBConfig((Guid)dbConfigId, cabinet.DrawerBoxOptions, connection, trx);
            }

            InsertIntoProductTable(cabinet, orderId, connection, trx);
            InsertCabinet(cabinet, mdfConfigId, connection, trx);

            var parameters = new {
                ProductId = cabinet.Id,
                ToeType = cabinet.ToeType,
                TrashConfig = cabinet.TrashPulloutConfiguration,
                DrawerFaceHeight = cabinet.DrawerFaceHeight,
                DBConfigId = dbConfigId
            };

            connection.Execute("""
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