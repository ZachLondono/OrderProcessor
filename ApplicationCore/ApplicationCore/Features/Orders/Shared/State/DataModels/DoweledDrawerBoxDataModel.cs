using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.DrawerBoxes;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class DoweledDrawerBoxDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public Dimension Height { get; set; }
    public Dimension Width { get; set; }
    public Dimension Depth { get; set; }
    public string FrontMatName { get; set; } = string.Empty;
    public Dimension FrontMatThickness { get; set; }
    public bool FrontMatGraining { get; set; }
    public string BackMatName { get; set; } = string.Empty;
    public Dimension BackMatThickness { get; set; }
    public bool BackMatGraining { get; set; }
    public string SideMatName { get; set; } = string.Empty;
    public Dimension SideMatThickness { get; set; }
    public bool SideMatGraining { get; set; }
    public string BottomMatName { get; set; } = string.Empty;
    public Dimension BottomMatThickness { get; set; }
    public bool BottomMatGraining { get; set; }
    public bool MachineThicknessForUM { get; set; }
    public Dimension FrontBackHeightAdjustment { get; set; }
    public List<string> ProductionNotes { get; set; } = new();

    public static string GetQueryByOrderId =>
        """
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

            config.front_mat_name AS FrontMatName,
            config.front_mat_thickness AS FrontMatThickness,
            config.front_mat_graining AS FrontMatGraining,
            config.back_mat_name AS BackMatName,
            config.back_mat_thickness AS BackMatThickness,
            config.back_mat_graining AS BackMatGraining,
            config.side_mat_name AS SideMatName,
            config.side_mat_thickness AS SideMatThickness,
            config.side_mat_graining AS SideMatGraining,
            config.bottom_mat_name AS BottomMatName,
            config.bottom_mat_thickness AS BottomMatThickness,
            config.bottom_mat_graining AS BottomMatGraining,
            config.machine_thickness_for_um AS MachineThicknessForUM,
            config.frontback_height_adjustment AS FrontBackHeightAdjustment

        FROM doweled_drawer_products AS db_product

            JOIN products ON db_product.product_id = products.id
            JOIN doweled_drawer_box_configs AS config ON config.id = products.id

        WHERE products.order_id = @OrderId;
        """;

    public IProduct MapToProduct() {

        var frontMaterial = new DoweledDrawerBoxMaterial(FrontMatName, FrontMatThickness, FrontMatGraining);
        var backMaterial = new DoweledDrawerBoxMaterial(BackMatName, BackMatThickness, BackMatGraining);
        var sideMaterial = new DoweledDrawerBoxMaterial(SideMatName, SideMatThickness, SideMatGraining);
        var bottomMaterial = new DoweledDrawerBoxMaterial(BottomMatName, BottomMatThickness, BottomMatGraining);
        return new DoweledDrawerBoxProduct(Id, UnitPrice, Qty, Room, ProductNumber,
                                            Height, Width, Depth,
                                            frontMaterial, backMaterial, sideMaterial, bottomMaterial,
                                            MachineThicknessForUM, FrontBackHeightAdjustment) {
            ProductionNotes = ProductionNotes
        };

    }

}
