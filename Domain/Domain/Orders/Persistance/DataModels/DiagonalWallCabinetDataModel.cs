using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace Domain.Orders.Persistance.DataModels;

public class DiagonalWallCabinetDataModel : CabinetDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public Dimension RightWidth { get; set; }
    public Dimension RightDepth { get; set; }
    public HingeSide HingeSide { get; set; }
    public int DoorQty { get; set; }
    public Dimension DoorExtendDown { get; set; }
    public int AdjShelfQty { get; set; }
    public bool IsGarage { get; set; }
    public List<string> ProductionNotes { get; set; } = new();

    public IProduct MapToProduct() {

        var doorConfiguration = GetDoorConfiguration();

        var boxMaterial = new CabinetMaterial(BoxMatFinish, BoxFinishType, BoxMatCore);
        var finishMaterial = new CabinetFinishMaterial(FinishMatFinish, FinishFinishType, FinishMatCore, FinishMatPaint);

        return new WallDiagonalCornerCabinet(Id, Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, boxMaterial, finishMaterial, doorConfiguration, EdgeBandColor, RightSideType, LeftSideType, Comment,
            RightWidth, RightDepth, AdjShelfQty, HingeSide, DoorQty, DoorExtendDown) {
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
           	    cabinets.edge_banding_finish As EdgeBandColor,
           	    cabinets.left_side_type AS LeftSideType,
           	    cabinets.right_side_type AS RightSideType,
           	    cabinets.assembled,
           	    cabinets.comment,

           	    diagonal_wall_cabinets.right_width AS RightWidth,
                diagonal_wall_cabinets.right_depth AS RightDepth,
                diagonal_wall_cabinets.hinge_side AS HingeSide,
                diagonal_wall_cabinets.door_qty AS DoorQty,
                diagonal_wall_cabinets.door_extend_down AS DoorExtendDown,
                diagonal_wall_cabinets.adj_shelf_qty AS AdjShelfQty,
                diagonal_wall_cabinets.is_garage AS IsGarage,

           	    cabinets.mdf_config_id IS NOT NULL AS ContainsMDFDoor,
           	    mdf_door_configs.framing_bead AS FramingBead,
           	    mdf_door_configs.edge_detail AS EdgeDetail,
           	    mdf_door_configs.panel_detail AS PanelDetail,
           	    mdf_door_configs.thickness,
           	    mdf_door_configs.material,
           	    mdf_door_configs.panel_drop AS PanelDrop,
           	    mdf_door_configs.finish_type AS FinishType,
           	    mdf_door_configs.finish_color AS FinishColor,

           	    cabinets.slab_door_material_id IS NOT NULL AS ContainsSlabDoors,
                cabinet_slab_door_materials.core AS SlabDoorCore,
                cabinet_slab_door_materials.finish AS SlabDoorFinish,
                cabinet_slab_door_materials.finish_type AS SlabDoorFinishType,
                cabinet_slab_door_materials.paint AS SlabDoorPaint

            FROM diagonal_wall_cabinets

               JOIN products ON diagonal_wall_cabinets.product_id = products.id
               JOIN cabinets ON cabinets.product_id = diagonal_wall_cabinets.product_id
               LEFT JOIN mdf_door_configs ON cabinets.mdf_config_id = mdf_door_configs.id
               LEFT JOIN cabinet_slab_door_materials ON cabinets.slab_door_material_id = cabinet_slab_door_materials.id

            WHERE
           	    products.order_id = @OrderId;
           """;

}
