using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertProduct(CustomDrilledVerticalPanel verticalPanel, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertIntoProductTable(verticalPanel, orderId, connection, trx);

            var parameters = new {
                ProductId = verticalPanel.Id,
                Sku = verticalPanel.SKU,
                Width = verticalPanel.Width,
                Length = verticalPanel.Length,
                MaterialFinish = verticalPanel.Material.Finish,
                MaterialCore = verticalPanel.Material.Core,
                PaintColor = verticalPanel.Paint?.Color ?? null,
                PaintedSide = verticalPanel.Paint?.Side ?? PaintedSide.None,
                EdgeBandingFinish = verticalPanel.EdgeBandingColor,
                Comment = verticalPanel.Comment,
                DrillingType = verticalPanel.DrillingType,
                ExtendBack = verticalPanel.ExtendBack,
                ExtendFront = verticalPanel.ExtendFront,
                HoleDimFromBottom = verticalPanel.HoleDimensionFromBottom,
                HoleDimFromTop = verticalPanel.HoleDimensionFromTop,
                TransHoleDimFromBottom = verticalPanel.TransitionHoleDimensionFromBottom,
                TransHoleDimFromTop = verticalPanel.TransitionHoleDimensionFromTop,
                BottomNotchDepth = verticalPanel.BottomNotchDepth,
                BottomNotchHeight = verticalPanel.BottomNotchHeight
            };

            await connection.ExecuteAsync("""
                    INSERT INTO custom_drilled_vertical_panels
                        (product_id,
                        width,
                        length,
                        material_finish,
                        material_core,
                        paint_color,
                        painted_side,
                        edge_banding_finish,
                        comment,
                        drilling_type,
                        extend_back,
                        extend_front,
                        hole_dim_from_bottom,
                        hole_dim_from_top,
                        trans_hole_dim_from_bottom,
                        trans_hole_dim_from_top,
                        bottom_notch_depth,
                        bottom_notch_height)
                    VALUES
                        (@ProductId,
                        @Width,
                        @Length,
                        @MaterialFinish,
                        @MaterialCore,
                        @PaintColor,
                        @PaintedSide,
                        @EdgeBandingFinish,
                        @Comment,
                        @DrillingType,
                        @ExtendBack,
                        @ExtendFront,
                        @HoleDimFromBottom,
                        @HoleDimFromTop,
                        @TransHoleDimFromBottom,
                        @TransHoleDimFromTop,
                        @BottomNotchDepth,
                        @BottomNotchHeight);
                    """, parameters, trx);

        }

    }
}