using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Features.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class PieCutWallCabinetDataModel : CabinetDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public Dimension RightWidth { get; set; }
    public Dimension RightDepth { get; set; }
    public HingeSide HingeSide { get; set; }
    public Dimension DoorExtendDown { get; set; }
    public int AdjShelfQty { get; set; }

    public IProduct MapToProduct() {

        var mdfConfig = GetMDFDoorConfiguration();

        var boxMaterial = new CabinetMaterial(BoxMatFinish, BoxFinishType, BoxMatCore);
        var finishMaterial = new CabinetFinishMaterial(FinishMatFinish, FinishFinishType, FinishMatCore, FinishMatPaint);

        return new WallPieCutCornerCabinet(Id, Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, boxMaterial, finishMaterial, mdfConfig, EdgeBandColor, RightSideType, LeftSideType, Comment,
            RightWidth, RightDepth, AdjShelfQty, HingeSide, DoorExtendDown);
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

           	    pie_cut_wall_cabinets.right_width AS RightWidth,
                pie_cut_wall_cabinets.right_depth AS RightDepth,
                pie_cut_wall_cabinets.hinge_side AS HingeSide,
                pie_cut_wall_cabinets.door_extend_down AS DoorExtendDown,
                pie_cut_wall_cabinets.adj_shelf_qty AS AdjShelfQty,

           	    cabinets.mdf_config_id IS NOT NULL AS ContainsMDFDoor,
           	    mdf_door_configs.framing_bead AS FramingBead,
           	    mdf_door_configs.edge_detail AS EdgeDetail,
           	    mdf_door_configs.panel_detail AS PanelDetail,
           	    mdf_door_configs.thickness,
           	    mdf_door_configs.material,
           	    mdf_door_configs.panel_drop AS PanelDrop,
           	    mdf_door_configs.paint_color AS PaintColor

            FROM pie_cut_wall_cabinets

               JOIN products ON pie_cut_wall_cabinets.product_id = products.id
               JOIN cabinets ON cabinets.product_id = pie_cut_wall_cabinets.product_id
               LEFT JOIN mdf_door_configs ON cabinets.mdf_config_id = mdf_door_configs.id

            WHERE
           	    products.order_id = @OrderId;
           """;

}
