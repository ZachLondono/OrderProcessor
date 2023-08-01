using ApplicationCore.Features.Orders.Shared.Domain;
using ApplicationCore.Features.Orders.Shared.Domain.Enums;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using ApplicationCore.Shared.Domain;

namespace ApplicationCore.Features.Orders.Shared.State.DataModels;

internal class SinkCabinetDataModel : CabinetRollOutContainerDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public ToeType ToeType { get; set; } = ToeType.LegLevelers;
    public HingeSide HingeSide { get; set; }
    public int DoorQty { get; set; }
    public int FalseDrawerQty { get; set; }
    public Dimension DrawerFaceHeight { get; set; }
    public int AdjShelfQty { get; set; }
    public ShelfDepth ShelfDepth { get; set; }
    public bool TiltFront { get; set; }
    public bool ContainsScoopSides { get; set; }
    public double? ScoopDepth { get; set; }
    public double? ScoopFromFront { get; set; }
    public double? ScoopFromBack { get; set; }

    public IProduct MapToProduct() {

        var dbOptions = GetDrawerBoxOptions();
        var mdfConfig = GetMDFDoorConfiguration();

        var rollOuts = GetRollOutOptions();

        var boxMaterial = new CabinetMaterial(BoxMatFinish, BoxFinishType, BoxMatCore);
        var finishMaterial = new CabinetFinishMaterial(FinishMatFinish, FinishFinishType, FinishMatCore, FinishMatPaint);

        ScoopSides? scoops = null;
        if (ContainsScoopSides) {
            if (ScoopDepth is null || ScoopFromFront is null || ScoopFromBack is null) {
                throw new NullReferenceException("One or more sink scoop dimensions are null");
            }
            scoops = new(Dimension.FromMillimeters((double)ScoopDepth), Dimension.FromMillimeters((double)ScoopFromFront), Dimension.FromMillimeters((double)ScoopFromBack));
        }

        return new SinkCabinet(Id, Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, boxMaterial, finishMaterial, GetSlabDoorMaterial(), mdfConfig, EdgeBandColor, RightSideType, LeftSideType, Comment,
            ToeType, HingeSide, DoorQty, FalseDrawerQty, DrawerFaceHeight, AdjShelfQty, ShelfDepth, rollOuts, dbOptions, TiltFront, scoops);

    }

    public static string GetQueryByOrderId
        =>
        """
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

            sink_cabinets.toe_type AS ToeType,
            sink_cabinets.hinge_side AS HingeSide,
            sink_cabinets.door_qty AS DoorQty,
            sink_cabinets.false_drawer_qty FalseDrawerQty,
            sink_cabinets.drawer_face_height AS DrawerFaceHeight,
            sink_cabinets.adj_shelf_qty AS AdjShelfQty,
            sink_cabinets.shelf_depth AS ShelfDepth,
            sink_cabinets.tilt_front AS TiltFront,
            sink_cabinets.scoop_sides AS ContainsScoopSides,
            sink_cabinets.scoop_depth AS ScoopDepth,
            sink_cabinets.scoop_from_front AS ScoopFromFront,
            sink_cabinets.scoop_from_back AS ScoopFromBack,

            db_config.material AS DBMaterial,
            db_config.slide_type AS DBSlideType,

            sink_cabinets.rollout_positions AS ROPositions,
            sink_cabinets.rollout_block_type AS ROBlockType,
            sink_cabinets.rollout_scoop_front AS ROScoopFront,

            cabinets.mdf_config_id IS NOT NULL AS ContainsMDFDoor,
            mdf_door_configs.framing_bead AS FramingBead,
            mdf_door_configs.edge_detail AS EdgeDetail,
            mdf_door_configs.panel_detail AS PanelDetail,
            mdf_door_configs.thickness,
            mdf_door_configs.material,
            mdf_door_configs.panel_drop AS PanelDrop,
            mdf_door_configs.paint_color AS PaintColor

        FROM sink_cabinets

            JOIN products ON sink_cabinets.product_id = products.id
            JOIN cabinets ON cabinets.product_id = sink_cabinets.product_id
            JOIN cabinet_db_configs AS db_config ON sink_cabinets.db_config_id = db_config.id
            LEFT JOIN mdf_door_configs ON cabinets.mdf_config_id = mdf_door_configs.id

        WHERE
            products.order_id = @OrderId;
        """;

}