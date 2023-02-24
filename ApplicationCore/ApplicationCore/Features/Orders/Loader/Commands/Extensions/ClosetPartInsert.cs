using ApplicationCore.Features.Orders.Shared.Domain.Products;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(ClosetPart closetPart, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertIntoProductTable(closetPart, orderId, connection, trx);

            var parameters = new {
                ProductId = closetPart.Id,
                Room = closetPart.Room,
                Sku = closetPart.SKU,
                Width = closetPart.Width,
                Length = closetPart.Length,
                MaterialFinish = closetPart.Material.Finish,
                MaterialCore = closetPart.Material.Core,
                PaintColor = closetPart.Material.PaintColor,
                EdgeBandingFinish = closetPart.EdgeBandingColor,
                Comment = closetPart.Comment,
                Parameters = (IDictionary<string, string>)closetPart.Parameters
            };

            await connection.ExecuteAsync("""
                    INSERT INTO closet_parts
                        (product_id,
                        room,
                        sku,
                        width,
                        length,
                        material_finish,
                        material_core,
                        paint_color,
                        edge_banding_finish,
                        comment,
                        parameters)
                    VALUES
                        (@ProductId,
                        @Room,
                        @Sku,
                        @Width,
                        @Length,
                        @MaterialFinish,
                        @MaterialCore,
                        @PaintColor,
                        @EdgeBandingFinish,
                        @Comment,
                        @Parameters);
                    """, parameters, trx);

        }

    }

}
