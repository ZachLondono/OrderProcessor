using Domain.Orders.Entities.Products.Cabinets;
using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(BasePieCutCornerCabinet cabinet, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

            Guid? mdfConfigId = null;
            if (cabinet.DoorConfiguration.TryGetMDFOptions(out var mdfConfig)) {
                mdfConfigId = Guid.NewGuid();
                InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            InsertIntoProductTable(cabinet, orderId, connection, trx);
            InsertCabinet(cabinet, mdfConfigId, connection, trx);

            var parameters = new {
                ProductId = cabinet.Id,
                ToeType = cabinet.ToeType,
                RightWidth = cabinet.RightWidth,
                RightDepth = cabinet.RightDepth,
                HingeSide = cabinet.HingeSide,
                AdjShelfQty = cabinet.AdjustableShelves
            };

            connection.Execute("""
                    INSERT INTO pie_cut_base_cabinets
                        (product_id,
                        toe_type,
                        right_width,
                        right_depth,
                        hinge_side,
                        adj_shelf_qty)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @RightWidth,
                        @RightDepth,
                        @HingeSide,
                        @AdjShelfQty);
                    """, parameters, trx);

        }

    }

}
