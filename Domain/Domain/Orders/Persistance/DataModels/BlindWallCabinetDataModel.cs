using Domain.Orders.Enums;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using Domain.Orders.Entities.Products.Cabinets;

namespace Domain.Orders.Persistance.DataModels;

public class BlindWallCabinetDataModel : CabinetDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public int AdjShelfQty { get; set; }
    public ShelfDepth ShelfDepth { get; set; }
    public BlindSide BlindSide { get; set; }
    public Dimension BlindWidth { get; set; }
    public Dimension DoorExtendDown { get; set; }
    public int DoorQty { get; set; }
    public HingeSide HingeSide { get; set; }
    public List<string> ProductionNotes { get; set; } = new();

    public IProduct MapToProduct() {

        var doorConfiguration = GetDoorConfiguration();

        var doors = new BlindCabinetDoors(HingeSide, DoorQty);

        var boxMaterial = new CabinetMaterial(BoxMatFinish, BoxFinishType, BoxMatCore);
        var finishMaterial = new CabinetFinishMaterial(FinishMatFinish, FinishFinishType, FinishMatCore, FinishMatPaint);

        return new BlindWallCabinet(Id, Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, boxMaterial, finishMaterial, doorConfiguration, EdgeBandColor, RightSideType, LeftSideType, Comment,
            doors, BlindSide, BlindWidth, AdjShelfQty, DoorExtendDown) {
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

                blind_wall_cabinets.blind_side AS BlindSide,
                blind_wall_cabinets.blind_width AS BlindWidth,
                blind_wall_cabinets.adj_shelf_qty AS AdjShelfQty,
                blind_wall_cabinets.door_extend_down AS DoorExtendDown,
                blind_wall_cabinets.door_qty AS DoorQty,
                blind_wall_cabinets.hinge_side AS HingeSide,

           	    cabinets.mdf_config_id IS NOT NULL AS ContainsMDFDoor,
           	    mdf_door_configs.framing_bead AS FramingBead,
           	    mdf_door_configs.edge_detail AS EdgeDetail,
           	    mdf_door_configs.panel_detail AS PanelDetail,
           	    mdf_door_configs.thickness,
           	    mdf_door_configs.material,
           	    mdf_door_configs.panel_drop AS PanelDrop,
           	    mdf_door_configs.paint_color AS PaintColor,

           	    cabinets.slab_door_material_id IS NOT NULL AS ContainsSlabDoors,
                cabinet_slab_door_materials.core AS SlabDoorCore,
                cabinet_slab_door_materials.finish AS SlabDoorFinish,
                cabinet_slab_door_materials.finish_type AS SlabDoorFinishType,
                cabinet_slab_door_materials.paint AS SlabDoorPaint

            FROM blind_wall_cabinets

               JOIN products ON blind_wall_cabinets.product_id = products.id
               JOIN cabinets ON cabinets.product_id = blind_wall_cabinets.product_id
               LEFT JOIN mdf_door_configs ON cabinets.mdf_config_id = mdf_door_configs.id
               LEFT JOIN cabinet_slab_door_materials ON cabinets.slab_door_material_id = cabinet_slab_door_materials.id

            WHERE
           	    products.order_id = @OrderId;
           """;

}


