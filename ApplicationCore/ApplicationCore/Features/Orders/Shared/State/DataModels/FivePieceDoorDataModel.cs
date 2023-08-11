using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Doors;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class FivePieceDoorDataModel : ProductDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public Dimension Width { get; set; }
    public Dimension Height { get; set; }
    public Dimension TopRail { get; set; }
    public Dimension BottomRail { get; set; }
    public Dimension LeftStile { get; set; }
    public Dimension RightStile { get; set; }
    public Dimension FrameThickness { get; set; }
    public Dimension PanelThickness { get; set; }
    public string Material { get; set; } = string.Empty;

    public static string GetQueryByOrderId =>
        """
        SELECT

            products.id,
            products.qty,
            products.unit_price AS UnitPrice,
            products.product_number AS ProductNumber,
            products.room,

            fpd_product.width,
            fpd_product.height,
            fpd_product.top_rail AS TopRail,
            fpd_product.bottom_rail AS BottomRail,
            fpd_product.left_stile AS LeftStile,
            fpd_product.right_stile AS RightStile,

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
        return new FivePieceDoorProduct(Id, Qty, UnitPrice, ProductNumber, Room, Width, Height, frameSize, FrameThickness, PanelThickness, Material);
    }

}
