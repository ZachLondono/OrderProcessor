using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;
using Domain.Orders.Entities;

namespace Domain.Orders.Persistance.DataModels;

public class TrashCabinetDataModel : CabinetDrawerBoxContainerDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public ToeType ToeType { get; set; } = ToeType.LegLevelers;
    public TrashPulloutConfiguration TrashConfig { get; set; }
    public Dimension DrawerFaceHeight { get; set; }

    public IProduct MapToProduct(IEnumerable<ProductionNote> productionNotes) {

        var dbOptions = GetDrawerBoxOptions();
        var mdfConfig = GetMDFDoorConfiguration();

        var boxMaterial = new CabinetMaterial(BoxMatFinish, BoxFinishType, BoxMatCore);
        var finishMaterial = new CabinetFinishMaterial(FinishMatFinish, FinishFinishType, FinishMatCore, FinishMatPaint);

        return new TrashCabinet(Id, Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, boxMaterial, finishMaterial, GetSlabDoorMaterial(), mdfConfig, EdgeBandColor, RightSideType, LeftSideType, Comment,
            DrawerFaceHeight, TrashConfig, dbOptions, ToeType) {
            ProductionNotes = productionNotes.ToList()
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

           	    trash_cabinets.toe_type AS ToeType,
                trash_cabinets.trash_config AS TrashConfig,
           	    trash_cabinets.drawer_face_height AS DrawerFaceHeight,

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

            FROM trash_cabinets

               JOIN products ON trash_cabinets.product_id = products.id
               JOIN cabinets ON cabinets.product_id = trash_cabinets.product_id
               JOIN cabinet_db_configs AS db_config ON trash_cabinets.db_config_id = db_config.id
               LEFT JOIN mdf_door_configs ON cabinets.mdf_config_id = mdf_door_configs.id

            WHERE
           	    products.order_id = @OrderId;
           """;

}