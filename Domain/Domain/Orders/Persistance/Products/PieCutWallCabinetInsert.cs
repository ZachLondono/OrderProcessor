using Domain.Orders.Entities.Products.Cabinets;
using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance.Products;

public static partial class ProductsPersistance {

    public static void InsertProduct(WallPieCutCornerCabinet cabinet, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

        Guid? mdfConfigId = null;
        if (cabinet.DoorConfiguration.TryGetMDFOptions(out var mdfConfig)) {
            mdfConfigId = Guid.NewGuid();
            InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
        }

        InsertIntoProductTable(cabinet, orderId, connection, trx);
        InsertCabinet(cabinet, mdfConfigId, connection, trx);

        var parameters = new {
            ProductId = cabinet.Id,
            RightWidth = cabinet.RightWidth,
            RightDepth = cabinet.RightDepth,
            HingeSide = cabinet.HingeSide,
            DoorExtendDown = cabinet.ExtendedDoor,
            AdjShelfQty = cabinet.AdjustableShelves
        };

        connection.Execute("""
                    INSERT INTO pie_cut_wall_cabinets
                        (product_id,
                        right_width,
                        right_depth,
                        hinge_side,
                        door_extend_down,
                        adj_shelf_qty)
                    VALUES
                        (@ProductId,
                        @RightWidth,
                        @RightDepth,
                        @HingeSide,
                        @DoorExtendDown,
                        @AdjShelfQty);
                    """, parameters, trx);

    }

}