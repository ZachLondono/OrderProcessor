using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Closets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class ZargenDrawerDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

	public string Sku { get; set; } = string.Empty;
	public Dimension OpeningWidth { get; set; }
	public Dimension Height { get; set; }
	public Dimension Depth { get; set; }
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
            products.room,

            zargen_drawers.sku,
            zargen_drawers.opening_width,
            zargen_drawers.height,
            zargen_drawers.depth,
            zargen_drawers.material_finish AS MaterialFinish,
            zargen_drawers.material_core AS MaterialCore,
            zargen_drawers.paint_color AS PaintColor,
            zargen_drawers.painted_side AS PaintedSide,
            zargen_drawers.edge_banding_finish AS EdgeBandingFinish,
            zargen_drawers.comment,
            zargen_drawers.parameters
        
         FROM zargen_drawers 
        
             JOIN products ON zargen_drawers.product_id = products.id
        
         WHERE
            products.order_id = @OrderId;
        """;

	public IProduct MapToProduct() {
		ClosetPaint? paint = PaintColor is null ? null : new(PaintColor, PaintedSide);
		return new ZargenDrawer(Id, Qty, UnitPrice, ProductNumber, Room, Sku, OpeningWidth, Height, Depth, new(MaterialFinish, MaterialCore), paint, EdgeBandingFinish, Comment, Parameters.AsReadOnly());
	}

}