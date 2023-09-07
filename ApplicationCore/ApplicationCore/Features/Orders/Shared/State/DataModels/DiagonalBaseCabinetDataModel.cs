using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class DiagonalBaseCabinetDataModel : CabinetDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public ToeType ToeType { get; set; } = ToeType.LegLevelers;
    public Dimension RightWidth { get; set; }
    public Dimension RightDepth { get; set; }
    public HingeSide HingeSide { get; set; }
    public int DoorQty { get; set; }
    public int AdjShelfQty { get; set; }
    public bool IsGarage { get; set; }
    public List<string> ProductionNotes { get; set; } = new();

    public IProduct MapToProduct() {

        var mdfConfig = GetMDFDoorConfiguration();

        var boxMaterial = new CabinetMaterial(BoxMatFinish, BoxFinishType, BoxMatCore);
        var finishMaterial = new CabinetFinishMaterial(FinishMatFinish, FinishFinishType, FinishMatCore, FinishMatPaint);

        return new BaseDiagonalCornerCabinet(Id, Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, boxMaterial, finishMaterial, GetSlabDoorMaterial(), mdfConfig, EdgeBandColor, RightSideType, LeftSideType, Comment,
            RightWidth, RightDepth, ToeType, AdjShelfQty, HingeSide, DoorQty) {
            IsGarage = IsGarage,
            ProductionNotes = ProductionNotes
        };

    }

    public static string GetQueryByOrderId
        => """
            SELECT 

           	    products.id,
           	    products.qty,
           	    products.unit_price AS UnitPrice,
           	    products.product_number AS ProductNumber,
           	    products.room,
                products.production_notes AS ProductionNotes,

           	    cabinets.height,
           	    cabinets.width,
           	    cabinets.depth,
           	    cabinets.box_material_core AS BoxMatCore,
           	    cabinets.box_material_finish AS BoxMatFinish,
                cabinets.box_material_finish_type AS BoxFinishType,
           	    cabinets.finish_material_core AS FinishMatCore,
           	    cabinets.finish_material_finish AS FinishMatFinish,
           	    cabinets.finish_material_paint AS FinishMatPaint,
                cabinets.finish_material_finish_type AS FinishFinishType,
                cabinets.slab_door_core IS NOT NULL AS ContainsSlabDoors,
                cabinets.slab_door_core AS SlabDoorCore,
                cabinets.slab_door_finish AS SlabDoorFinish,
                cabinets.slab_door_finish_type AS SlabDoorFinishType,
                cabinets.slab_door_paint AS SlabDoorPaint,
           	    cabinets.edge_banding_finish As EdgeBandColor,
           	    cabinets.left_side_type AS LeftSideType,
           	    cabinets.right_side_type AS RightSideType,
           	    cabinets.assembled,
           	    cabinets.comment,

                diagonal_base_cabinets.toe_type AS ToeType,
           	    diagonal_base_cabinets.right_width AS RightWidth,
                diagonal_base_cabinets.right_depth AS RightDepth,
                diagonal_base_cabinets.hinge_side AS HingeSide,
                diagonal_base_cabinets.door_qty AS DoorQty,
                diagonal_base_cabinets.adj_shelf_qty AS AdjShelfQty,
                diagonal_base_cabinets.is_garage AS IsGarage,

           	    cabinets.mdf_config_id IS NOT NULL AS ContainsMDFDoor,
           	    mdf_door_configs.framing_bead AS FramingBead,
           	    mdf_door_configs.edge_detail AS EdgeDetail,
           	    mdf_door_configs.panel_detail AS PanelDetail,
           	    mdf_door_configs.thickness,
           	    mdf_door_configs.material,
           	    mdf_door_configs.panel_drop AS PanelDrop,
           	    mdf_door_configs.paint_color AS PaintColor

            FROM diagonal_base_cabinets

               JOIN products ON diagonal_base_cabinets.product_id = products.id
               JOIN cabinets ON cabinets.product_id = diagonal_base_cabinets.product_id
               LEFT JOIN mdf_door_configs ON cabinets.mdf_config_id = mdf_door_configs.id

            WHERE
           	    products.order_id = @OrderId;
           """;

}
