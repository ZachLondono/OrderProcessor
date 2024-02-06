using Domain.Orders.ValueObjects;
using Dapper;
using Domain.Orders.Entities.Products.Closets;
using System.Data;

namespace Domain.Orders.Persistance;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(ZargenDrawer zargenDrawer, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertIntoProductTable(zargenDrawer, orderId, connection, trx);

            var parameters = new {
                ProductId = zargenDrawer.Id,
                Sku = zargenDrawer.SKU,
                OpeningWidth = zargenDrawer.OpeningWidth,
                Height = zargenDrawer.Height,
                Depth = zargenDrawer.Depth,
                MaterialFinish = zargenDrawer.Material.Finish,
                MaterialCore = zargenDrawer.Material.Core,
                PaintColor = zargenDrawer.Paint?.Color ?? null,
                PaintedSide = zargenDrawer.Paint?.Side ?? PaintedSide.None,
                EdgeBandingFinish = zargenDrawer.EdgeBandingColor,
                Comment = zargenDrawer.Comment,
                Parameters = (IDictionary<string, string>)zargenDrawer.Parameters
            };

            await connection.ExecuteAsync("""
                    INSERT INTO zargen_drawers 
                        (product_id,
                        sku,
                        opening_width,
                        height,
                        depth,
                        material_finish,
                        material_core,
                        paint_color,
                        painted_side,
                        edge_banding_finish,
                        comment,
                        parameters)
                    VALUES
                        (@ProductId,
                        @Sku,
                        @OpeningWidth,
                        @Height,
                        @Depth,
                        @MaterialFinish,
                        @MaterialCore,
                        @PaintColor,
                        @PaintedSide,
                        @EdgeBandingFinish,
                        @Comment,
                        @Parameters);
                    """, parameters, trx);

        }

    }
}
