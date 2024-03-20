using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using Domain.Orders.Enums;

namespace Domain.Orders.Persistance.DataModels;

public class FivePieceDoorDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public Dimension Width { get; set; }
    public Dimension Height { get; set; }
    public Dimension TopRail { get; set; }
    public Dimension BottomRail { get; set; }
    public Dimension LeftStile { get; set; }
    public Dimension RightStile { get; set; }
    public DoorType DoorType { get; set; }
    public Dimension FrameThickness { get; set; }
    public Dimension PanelThickness { get; set; }
    public string Material { get; set; } = string.Empty;
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

            fpd_product.width,
            fpd_product.height,
            fpd_product.top_rail AS TopRail,
            fpd_product.bottom_rail AS BottomRail,
            fpd_product.left_stile AS LeftStile,
            fpd_product.right_stile AS RightStile,
            fpd_product.type AS DoorType,

            config.frame_thickness AS FrameThickness,
            config.panel_thickness AS PanelThickness,
            config.material

        FROM five_piece_door_products AS fpd_product

            JOIN products ON fpd_product.product_id = products.id
            JOIN five_piece_door_configs AS config ON config.id = products.id

        WHERE products.order_id = @OrderId;

        """;

    public IProduct MapToProduct() {
        DoorFrame frameSize = new() {
            TopRail = TopRail,
            BottomRail = BottomRail,
            LeftStile = LeftStile,
            RightStile = RightStile
        };
        return new FivePieceDoorProduct(Id, Qty, UnitPrice, ProductNumber, Room, Width, Height, frameSize, FrameThickness, PanelThickness, Material, DoorType) {
            ProductionNotes = ProductionNotes
        };
    }

}
