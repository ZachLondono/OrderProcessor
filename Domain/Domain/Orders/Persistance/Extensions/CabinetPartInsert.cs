using Dapper;
using Domain.Orders.Entities.Products.Cabinets;
using System.Data;

namespace Domain.Orders.Persistance;
public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(CabinetPart cabinetPart, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertIntoProductTable(cabinetPart, orderId, connection, trx);

            var parameters = new {
                ProductId = cabinetPart.Id,
                Sku = cabinetPart.SKU,
                MaterialCore = cabinetPart.Material.Core,
                MaterialFinish = cabinetPart.Material.Finish,
                MaterialFinishType = cabinetPart.Material.FinishType,
                EdgeBandingFinish = cabinetPart.EdgeBandingColor,
                Comment = cabinetPart.Comment,
                Parameters = cabinetPart.Parameters
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO cabinet_parts
                    (product_id,
                    sku,
                    material_core,
                    material_finish,
                    material_finish_type,
                    edge_banding_finish,
                    comment,
                    parameters)
                VALUES
                    (@ProductId,
                    @Sku,
                    @MaterialCore,
                    @MaterialFinish,
                    @MaterialFinishType,
                    @EdgeBandingFinish,
                    @Comment,
                    @Parameters); 
                """, parameters, trx);

        }

    }
}
