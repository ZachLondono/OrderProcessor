using Domain.Orders.Entities.Products.Cabinets;
using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static void InsertProduct(BaseDiagonalCornerCabinet cabinet, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

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
                DoorQty = cabinet.DoorQty,
                AdjShelfQty = cabinet.AdjustableShelves,
                IsGarage = cabinet.IsGarage
            };

            connection.Execute("""
                    INSERT INTO diagonal_base_cabinets
                        (product_id,
                        toe_type,
                        right_width,
                        right_depth,
                        hinge_side,
                        door_qty,
                        adj_shelf_qty,
                        is_garage)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @RightWidth,
                        @RightDepth,
                        @HingeSide,
                        @DoorQty,
                        @AdjShelfQty,
                        @IsGarage);
                    """, parameters, trx);

        }

    }

}
