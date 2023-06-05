using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class BlindBaseCabinetDataModel : CabinetDrawerBoxContainerDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public ToeType ToeType { get; set; } = ToeType.LegLevelers;
    public int AdjShelfQty { get; set; }
    public ShelfDepth ShelfDepth { get; set; }
    public BlindSide BlindSide { get; set; }
    public Dimension BlindWidth { get; set; }
    public int DoorQty { get; set; }
    public HingeSide HingeSide { get; set; }
    public int DrawerQty { get; set; }
    public Dimension DrawerFaceHeight { get; set; }

    public IProduct MapToProduct() {

        var dbOptions = GetDrawerBoxOptions();
        var mdfConfig = GetMDFDoorConfiguration();

        var doors = new BlindCabinetDoors(HingeSide, DoorQty);

        var boxMaterial = new CabinetMaterial(BoxMatFinish, BoxFinishType, BoxMatCore);
        var finishMaterial = new CabinetFinishMaterial(FinishMatFinish, FinishFinishType, FinishMatCore, FinishMatPaint);

        var drawers = new HorizontalDrawerBank() {
            FaceHeight = DrawerFaceHeight,
            Quantity = DrawerQty
        };

        return new BlindBaseCabinet(Id, Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, boxMaterial, finishMaterial, GetSlabDoorMaterial(), mdfConfig, EdgeBandColor, RightSideType, LeftSideType, Comment,
            doors, BlindSide, BlindWidth, AdjShelfQty, ShelfDepth, drawers, ToeType, dbOptions);

    }

    public static string GetQueryByOrderId
        => """
            SELECT 

           	    products.id,
           	    products.qty,
           	    products.unit_price AS UnitPrice,
           	    products.product_number AS ProductNumber,

           	    cabinets.height,
           	    cabinets.width,
           	    cabinets.depth,
           	    cabinets.box_material_core AS BoxMatCore,
           	    cabinets.box_material_finish AS BoxMatFinish,
           	    cabinets.finish_material_core AS FinishMatCore,
           	    cabinets.finish_material_finish AS FinishMatFinish,
           	    cabinets.finish_material_paint AS FinishMatPaint,
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
           	    cabinets.room,

                blind_base_cabinets.toe_type AS ToeType,
                blind_base_cabinets.blind_side AS BlindSide,
                blind_base_cabinets.blind_width AS BlindWidth,
                blind_base_cabinets.shelf_depth AS ShelfDepth,
                blind_base_cabinets.adj_shelf_qty AS AdjShelfQty,
                blind_base_cabinets.door_qty AS DoorQty,
                blind_base_cabinets.hinge_side AS HingeSide,
                blind_base_cabinets.drawer_qty AS DrawerQty,
                blind_base_cabinets.drawer_face_height AS DrawerFaceHeight,
           
           	    db_config.material AS DBMaterial,
           	    db_config.slide_type AS DBSlideType,
           
           	    cabinets.mdf_config_id IS NOT NULL AS ContainsMDFDoor,
           	    mdf_door_configs.framing_bead AS FramingBead,
           	    mdf_door_configs.edge_detail AS EdgeDetail,
           	    mdf_door_configs.panel_detail AS PanelDetail,
           	    mdf_door_configs.thickness,
           	    mdf_door_configs.material,
           	    mdf_door_configs.panel_drop AS PanelDrop,
           	    mdf_door_configs.paint_color AS PaintColor

            FROM blind_base_cabinets

               JOIN products ON blind_base_cabinets.product_id = products.id
               JOIN cabinets ON cabinets.product_id = blind_base_cabinets.product_id
               JOIN cabinet_db_configs AS db_config ON blind_base_cabinets.db_config_id = db_config.id
               LEFT JOIN mdf_door_configs ON cabinets.mdf_config_id = mdf_door_configs.id

            WHERE
           	    products.order_id = @OrderId;
           """;

}


