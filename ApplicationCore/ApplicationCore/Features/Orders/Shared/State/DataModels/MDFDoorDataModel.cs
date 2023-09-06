using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class MDFDoorDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public string Note { get; set; } = string.Empty;
    public Dimension Height { get; set; }
    public Dimension Width { get; set; }
    public DoorType Type { get; set; }
    public Dimension TopRail { get; set; }
    public Dimension BottomRail { get; set; }
    public Dimension LeftStile { get; set; }
    public Dimension RightStile { get; set; }
    public DoorOrientation Orientation { get; set; }
    public AdditionalOpening[] AdditionalOpenings { get; set; } = Array.Empty<AdditionalOpening>();
    public List<string> ProductionNotes { get; set; } = new();

    public string FramingBead { get; set; } = string.Empty;
    public string EdgeDetail { get; set; } = string.Empty;
    public string PanelDetail { get; set; } = string.Empty;
    public Dimension Thickness { get; set; }
    public string Material { get; set; } = string.Empty;
    public Dimension PanelDrop { get; set; }
    public string? PaintColor { get; set; }

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

        	mdf_product.note,
        	mdf_product.height,
        	mdf_product.width,
        	mdf_product.type,
        	mdf_product.top_rail AS TopRail,
        	mdf_product.bottom_rail AS BottomRail,
        	mdf_product.left_stile AS LeftStile,
        	mdf_product.right_stile AS RightStile,
            mdf_product.orientation AS Orientation,

        	mdf_config.framing_bead AS FramingBead,
        	mdf_config.edge_detail AS EdgeDetail,
        	mdf_config.panel_detail AS PanelDetail,
        	mdf_config.thickness,
        	mdf_config.material,
        	mdf_config.panel_drop AS PanelDrop,
        	mdf_config.paint_color AS PaintColor

        FROM mdf_door_products AS mdf_product

        	JOIN products ON mdf_product.product_id = products.id
        	JOIN mdf_door_configs AS mdf_config ON mdf_config.id = products.id

        WHERE products.order_id = @OrderId;
        """;

    public static string GetAdditionalOpeningsQueryByProductId
        =>
        """
        SELECT
            rail AS RailWidth,
            opening AS OpeningHeight
        FROM mdf_door_openings
        WHERE product_id = @ProductId;
        """;

    public IProduct MapToProduct() {

        var frameSize = new DoorFrame() {
            TopRail = TopRail,
            BottomRail = BottomRail,
            LeftStile = LeftStile,
            RightStile = RightStile
        };

        return new MDFDoorProduct(Id, UnitPrice, Room, Qty, ProductNumber, Type, Height, Width, Note, frameSize, Material, Thickness, FramingBead, EdgeDetail, PanelDetail, PanelDrop, Orientation, AdditionalOpenings, PaintColor) {
            ProductionNotes = ProductionNotes
        };

    }

}
