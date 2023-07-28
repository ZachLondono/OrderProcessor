using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(BasePieCutCornerCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            Guid? mdfConfigId = null;
            var mdfConfig = cabinet.MDFDoorOptions;
            if (mdfConfig is not null) {
                mdfConfigId = Guid.NewGuid();
                await InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            await InsertIntoProductTable(cabinet, orderId, connection, trx);
            await InsertCabinet(cabinet, mdfConfigId, connection, trx);

            var parameters = new {
                ProductId = cabinet.Id,
                ToeType = cabinet.ToeType,
                RightWidth = cabinet.RightWidth,
                RightDepth = cabinet.RightDepth,
                HingeSide = cabinet.HingeSide,
                AdjShelfQty = cabinet.AdjustableShelves
            };

            await connection.ExecuteAsync("""
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
