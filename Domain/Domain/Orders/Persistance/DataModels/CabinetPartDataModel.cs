using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.Entities.Products;
using Domain.Orders.Entities;

namespace Domain.Orders.Persistance.DataModels;

public class CabinetPartDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public string Sku { get; set; } = string.Empty;
    public CabinetMaterialCore MaterialCore { get; set; }
    public string MaterialFinish { get; set; } = string.Empty;
    public CabinetMaterialFinishType MaterialFinishType { get; set; }
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

            cabinet_parts.sku,
            cabinet_parts.material_core AS MaterialCore,
            cabinet_parts.material_finish AS MaterialFinish,
            cabinet_parts.material_finish_type AS MaterialFinishType,
            cabinet_parts.edge_banding_finish AS EdgeBandingFinish,
            cabinet_parts.comment,
            cabinet_parts.parameters
        
         FROM cabinet_parts
        
             JOIN products ON cabinet_parts.product_id = products.id
        
         WHERE
            products.order_id = @OrderId;
        """;

    public IProduct MapToProduct(IEnumerable<ProductionNote> productionNotes) {
        return new CabinetPart(Id,
                               Qty,
                               UnitPrice,
                               ProductNumber,
                               Sku,
                               Room,
                               new(MaterialFinish, MaterialFinishType, MaterialCore),
                               EdgeBandingFinish,
                               Comment,
                               Parameters,
                               productionNotes.ToList());
    }

}