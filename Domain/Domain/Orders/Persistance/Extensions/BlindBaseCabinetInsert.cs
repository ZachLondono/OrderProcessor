using Domain.Orders.Entities.Products.Cabinets;
using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(BlindBaseCabinet cabinet, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            Guid? mdfConfigId = null;
            var mdfConfig = cabinet.MDFDoorOptions;
            if (mdfConfig is not null) {
                mdfConfigId = Guid.NewGuid();
                InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            Guid? dbConfigId = null;
            if (cabinet.DrawerBoxOptions is not null) {
                dbConfigId = Guid.NewGuid();
                InsertCabinetDBConfig((Guid) dbConfigId, cabinet.DrawerBoxOptions, connection, trx);
            }

            InsertIntoProductTable(cabinet, orderId, connection, trx);
            InsertCabinet(cabinet, mdfConfigId, connection, trx);

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
                DBConfigId = dbConfigId,
                IsGarage = cabinet.IsGarage
            };

            connection.Execute("""
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
                        db_config_id,
                        is_garage)
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
                        @DBConfigId,
                        @IsGarage);
                    """, parameters, trx);

        }

    }

}
