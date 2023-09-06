using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class DovetailDrawerBoxDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public Dimension Height { get; set; }
    public Dimension Width { get; set; }
    public Dimension Depth { get; set; }
    public string Note { get; set; } = string.Empty;
    public IDictionary<string, string> LabelFields { get; set; } = new Dictionary<string, string>();
    public List<string> ProductionNotes { get; set; } = new();

    public string FrontMaterial { get; set; } = string.Empty;
    public string BackMaterial { get; set; } = string.Empty;
    public string SideMaterial { get; set; } = string.Empty;
    public string BottomMaterial { get; set; } = string.Empty;
    public string Clips { get; set; } = string.Empty;
    public string Notches { get; set; } = string.Empty;
    public string Accessory { get; set; } = string.Empty;
    public LogoPosition Logo { get; set; }
    public bool PostFinish { get; set; }
    public bool ScoopFront { get; set; }
    public bool FaceMountingHoles { get; set; }
    public bool Assembled { get; set; }

    public static string GetQueryByOrderId
        => """
            SELECT

            	products.id,
                products.qty,
                products.unit_price AS UnitPrice,
                products.product_number AS ProductNumber,
                products.room,
                products.production_notes AS ProductionNotes,

            	db_product.height,
            	db_product.width,
            	db_product.depth,
            	db_product.note,
            	db_product.label_fields,

            	db_config.front_material AS FrontMaterial,
            	db_config.back_material AS BackMaterial,
            	db_config.side_material AS SideMaterial,
            	db_config.bottom_material AS BottomMaterial,
            	db_config.clips,
            	db_config.notches,
            	db_config.accessory,
            	db_config.logo,
            	db_config.post_finish AS PostFinish,
            	db_config.scoop_front AS ScoopFront,
            	db_config.face_mounting_holes AS FaceMountingHoles,
            	db_config.assembled

            FROM dovetail_drawer_products AS db_product

            	JOIN products ON db_product.product_id = products.id
            	JOIN dovetail_drawer_box_configs AS db_config ON db_config.id = products.id

            WHERE products.order_id = @OrderId;
            """;

    public IProduct MapToProduct() {

        var options = new DovetailDrawerBoxConfig(FrontMaterial, BackMaterial, SideMaterial, BottomMaterial, Clips, Notches, Accessory, Logo, PostFinish, ScoopFront, FaceMountingHoles, Assembled, null, null);

        var box = new DovetailDrawerBoxProduct(Id, UnitPrice, Qty, Room, ProductNumber, Height, Width, Depth, Note, LabelFields.AsReadOnly(), options) {
            ProductionNotes = ProductionNotes
        };

        return box;

    }

}
