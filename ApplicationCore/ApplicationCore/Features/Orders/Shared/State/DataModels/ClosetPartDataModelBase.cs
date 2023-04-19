using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class ClosetPartDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public string Room { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public Dimension Width { get; set; }
    public Dimension Length { get; set; }
    public string MaterialFinish { get; set; } = string.Empty;
    public ClosetMaterialCore MaterialCore { get; set; }
    public string? PaintColor { get; set; }
    public PaintedSide PaintedSide { get; set; }
    public string EdgeBandingFinish { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

    public static string GetQueryByOrderId
        =>
        """
        SELECT 
        
        	products.id,
        	products.qty,
        	products.unit_price AS UnitPrice,
        	products.product_number AS ProductNumber,

            closet_parts.room,
            closet_parts.sku,
            closet_parts.width,
            closet_parts.length,
            closet_parts.material_finish AS MaterialFinish,
            closet_parts.material_core AS MaterialCore,
            closet_parts.paint_color AS PaintColor,
            closet_parts.painted_side AS PaintedSide,
            closet_parts.edge_banding_finish AS EdgeBandingFinish,
            closet_parts.comment,
            closet_parts.parameters
        
         FROM closet_parts
        
             JOIN products ON closet_parts.product_id = products.id
        
         WHERE
            products.order_id = @OrderId;
        """;

    public IProduct MapToProduct() {
        ClosetPaint? paint = PaintColor is null ? null : new(PaintColor, PaintedSide);
        return new ClosetPart(Id, Qty, UnitPrice, ProductNumber, Room, Sku, Width, Length, new(MaterialFinish, MaterialCore), paint, EdgeBandingFinish, Comment, Parameters.AsReadOnly());
    }

}