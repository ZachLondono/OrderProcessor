using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class CabinetPartDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public string Sku { get; set; } = string.Empty;
    public CabinetMaterialCore MaterialCore { get; set; }
    public string MaterialFinish { get; set; } = string.Empty;
    public CabinetMaterialFinishType MaterialFinishType { get; set; }
    public string EdgeBandingFinish { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
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

            closet_parts.sku,
            closet_parts.material_core AS MaterialCore,
            closet_parts.material_finish AS MaterialFinish,
            closet_parts.material_finish_type AS MaterialFinishType,
            closet_parts.edge_banding_finish AS EdgeBandingFinish,
            closet_parts.comment,
            closet_parts.parameters
        
         FROM cabinet_parts
        
             JOIN products ON cabinet_parts.product_id = products.id
        
         WHERE
            products.order_id = @OrderId;
        """;

    public IProduct MapToProduct() {
        return new CabinetPart(Id, Qty, UnitPrice, ProductNumber, Room, Sku, new(MaterialFinish, MaterialFinishType, MaterialCore), EdgeBandingFinish, Comment, Parameters, ProductionNotes);
    }

}