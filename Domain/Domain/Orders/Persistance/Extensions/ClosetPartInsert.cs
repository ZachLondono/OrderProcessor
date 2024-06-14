using Domain.Orders.ValueObjects;
using Dapper;
using Domain.Orders.Entities.Products.Closets;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(ClosetPart closetPart, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertIntoProductTable(closetPart, orderId, connection, trx);

            var parameters = new {
                ProductId = closetPart.Id,
                Sku = closetPart.SKU,
                Width = closetPart.Width,
                Length = closetPart.Length,
                MaterialFinish = closetPart.Material.Finish,
                MaterialCore = closetPart.Material.Core,
                PaintColor = closetPart.Paint?.Color ?? null,
                PaintedSide = closetPart.Paint?.Side ?? PaintedSide.None,
                EdgeBandingFinish = closetPart.EdgeBandingColor,
                Comment = closetPart.Comment,
                InstallCams = closetPart.InstallCams,
                Parameters = (IDictionary<string, string>)closetPart.Parameters
            };

            await connection.ExecuteAsync("""
                    INSERT INTO closet_parts
                        (product_id,
                        sku,
                        width,
                        length,
                        material_finish,
                        material_core,
                        paint_color,
                        painted_side,
                        edge_banding_finish,
                        comment,
                        install_cams,
                        parameters)
                    VALUES
                        (@ProductId,
                        @Sku,
                        @Width,
                        @Length,
                        @MaterialFinish,
                        @MaterialCore,
                        @PaintColor,
                        @PaintedSide,
                        @EdgeBandingFinish,
                        @Comment,
                        @InstallCams,
                        @Parameters);
                    """, parameters, trx);

        }

    }
}


