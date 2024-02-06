using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace Domain.Orders.Persistance.DataModels;

public class CustomDrilledVerticalPanelDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public string? PaintColor { get; set; }
    public Dimension Width { get; set; }
    public Dimension Length { get; set; }
    public string MaterialFinish { get; set; } = string.Empty;
    public ClosetMaterialCore MaterialCore { get; set; }
    public PaintedSide PaintedSide { get; set; }
    public string EdgeBandingFinish { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public ClosetVerticalDrillingType DrillingType { get; set; }
    public Dimension ExtendBack { get; set; }
    public Dimension ExtendFront { get; set; }
    public Dimension HoleDimensionFromBottom { get; set; }
    public Dimension HoleDimensionFromTop { get; set; }
    public Dimension TransitionHoleDimensionFromBottom { get; set; }
    public Dimension TransitionHoleDimensionFromTop { get; set; }
    public Dimension BottomNotchDepth { get; set; }
    public Dimension BottomNotchHeight { get; set; }
    public Dimension LEDChannelOffFront { get; set; }
    public Dimension LEDChannelWidth { get; set; }
    public Dimension LEDChannelDepth { get; set; }
    public List<string> ProductionNotes { get; set; } = new();

    public static string GetQueryByOrderId
        =>
        """
        SELECT

        	products.id,
        	products.qty,
        	products.unit_price AS UnitPrice,
        	products.product_number AS ProductNumber,
            products.room,
            products.production_notes AS ProductionNotes,
            
            custom_drilled_vertical_panels.width,
            custom_drilled_vertical_panels.length,
            custom_drilled_vertical_panels.material_finish AS MaterialFinish,
            custom_drilled_vertical_panels.material_core AS MaterialCore,
            custom_drilled_vertical_panels.paint_color AS PaintColor,
            custom_drilled_vertical_panels.painted_side AS PaintedSide,
            custom_drilled_vertical_panels.edge_banding_finish As EdgeBandingFinish,
            custom_drilled_vertical_panels.comment,
            custom_drilled_vertical_panels.drilling_type AS DrillingType,
            custom_drilled_vertical_panels.extend_back AS ExtendBack,
            custom_drilled_vertical_panels.extend_front AS ExtendFront,
            custom_drilled_vertical_panels.hole_dim_from_bottom As HoleDimensionFromBottom,
            custom_drilled_vertical_panels.hole_dim_from_top AS HoleDimensionFromTop,
            custom_drilled_vertical_panels.trans_hole_dim_from_bottom AS TransitionHoleDimensionFromBottom,
            custom_drilled_vertical_panels.trans_hole_dim_from_top AS TransitionHoleDimensionFromTop,
            custom_drilled_vertical_panels.bottom_notch_depth AS BottomNotchDepth,
            custom_drilled_vertical_panels.bottom_notch_height AS BottomNotchHeight,
            custom_drilled_vertical_panels.led_channel_off_front AS LEDChannelOffFront, 
            custom_drilled_vertical_panels.led_channel_width AS LEDChannelWidth,
            custom_drilled_vertical_panels.led_channel_depth AS LEDChannelDepth 

        FROM custom_drilled_vertical_panels

            JOIN products ON custom_drilled_vertical_panels.product_id = products.id

        WHERE
            products.order_id = @OrderId;
        """;

    public IProduct MapToProduct() {
        ClosetPaint? paint = PaintColor is null ? null : new(PaintColor, PaintedSide);
        ClosetMaterial material = new(MaterialFinish, MaterialCore);
        return new CustomDrilledVerticalPanel(Id, Qty, UnitPrice, ProductNumber, Room, Width, Length, material, paint, EdgeBandingFinish, Comment, DrillingType, ExtendBack, ExtendFront, HoleDimensionFromBottom, HoleDimensionFromTop, TransitionHoleDimensionFromBottom, TransitionHoleDimensionFromTop, BottomNotchDepth, BottomNotchHeight, LEDChannelOffFront, LEDChannelWidth, LEDChannelDepth) {
            ProductionNotes = ProductionNotes
        };
    }

}