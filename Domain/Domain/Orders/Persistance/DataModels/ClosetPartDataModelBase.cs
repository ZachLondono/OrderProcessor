using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Closets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using Domain.Orders.Entities;

namespace Domain.Orders.Persistance.DataModels;

public class ClosetPartDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public string Sku { get; set; } = string.Empty;
    public Dimension Width { get; set; }
    public Dimension Length { get; set; }
    public string MaterialFinish { get; set; } = string.Empty;
    public ClosetMaterialCore MaterialCore { get; set; }
    public string? PaintColor { get; set; }
    public PaintedSide PaintedSide { get; set; }
    public string EdgeBandingFinish { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool InstallCams { get; set; }
    public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

    public static string GetQueryByOrderId
        =>
        """
        SELECT 
        
        	products.id,
        	products.qty,
        	products.unit_price AS UnitPrice,
        	products.product_number AS ProductNumber,
            products.room,

            closet_parts.sku,
            closet_parts.width,
            closet_parts.length,
            closet_parts.material_finish AS MaterialFinish,
            closet_parts.material_core AS MaterialCore,
            closet_parts.paint_color AS PaintColor,
            closet_parts.painted_side AS PaintedSide,
            closet_parts.edge_banding_finish AS EdgeBandingFinish,
            closet_parts.comment,
            closet_parts.install_cams AS InstallCams,
            closet_parts.parameters
        
         FROM closet_parts
        
             JOIN products ON closet_parts.product_id = products.id
        
         WHERE
            products.order_id = @OrderId;
        """;

    public IProduct MapToProduct(IEnumerable<ProductionNote> productionNotes) {
        ClosetPaint? paint = PaintColor is null ? null : new(PaintColor, PaintedSide);
        return new ClosetPart(Id, Qty, UnitPrice, ProductNumber, Room, Sku, Width, Length, new(MaterialFinish, MaterialCore), paint, EdgeBandingFinish, Comment, InstallCams, Parameters.AsReadOnly()) {
            ProductionNotes = productionNotes.ToList()
        };
    }

}
