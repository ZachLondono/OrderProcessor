using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class TrashCabinetDataModel : CabinetDrawerBoxContainerDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public ToeType ToeType { get; set; } = ToeType.LegLevelers;
    public TrashPulloutConfiguration TrashConfig { get; set; }
    public Dimension DrawerFaceHeight { get; set; }

    public IProduct MapToProduct() {

        var dbOptions = GetDrawerBoxOptions();
        var mdfConfig = GetMDFDoorConfiguration();

        var boxMaterial = new CabinetMaterial(BoxMatFinish, BoxMatCore);
        var finishMaterial = new CabinetFinishMaterial(FinishMatFinish, FinishMatCore, FinishMatPaint);

        return new TrashCabinet(Id, Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, boxMaterial, finishMaterial, mdfConfig, EdgeBandColor, RightSideType, LeftSideType, Comment,
            DrawerFaceHeight, TrashConfig, dbOptions, ToeType);

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
           	    cabinets.edge_banding_finish As EdgeBandColor,
           	    cabinets.left_side_type AS LeftSideType,
           	    cabinets.right_side_type AS RightSideType,
           	    cabinets.assembled,
           	    cabinets.comment,
           	    cabinets.room,

           	    trash_cabinets.toe_type AS ToeType,
                trash_cabinets.trash_config AS TrashConfig,
           	    trash_cabinets.drawer_face_height AS DrawerFaceHeight,

           	    db_config.material AS DBMaterial,
           	    db_config.slide_type AS DBSlideType,

           	    cabinets.mdf_config_id IS NULL AS ContainsMDFDoor,
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