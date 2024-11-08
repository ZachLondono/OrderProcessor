using Domain.Orders.Entities.Products.Cabinets;
using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance.Products;

public static partial class ProductsPersistance {

    public static void InsertProduct(DrawerBaseCabinet cabinet, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

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
            FaceHeights = cabinet.Drawers.FaceHeights,
            DBConfigId = dbConfigId,
            IsGarage = cabinet.IsGarage
        };

        connection.Execute("""
                    INSERT INTO drawer_base_cabinets
                        (product_id,
                        toe_type,
                        face_heights,
                        db_config_id,
                        is_garage)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @FaceHeights,
                        @DBConfigId,
                        @IsGarage);
                    """, parameters, trx);

    }

}
