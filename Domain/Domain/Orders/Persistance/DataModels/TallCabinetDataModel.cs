using Domain.Orders.Enums;
using Domain.Orders.Entities.Products.Cabinets;
using Domain.Orders.ValueObjects;
using Domain.Orders.Entities.Products;
using Domain.ValueObjects;

namespace Domain.Orders.Persistance.DataModels;

public class TallCabinetDataModel : CabinetRollOutContainerDataModelBase, IProductDataModel, IQueryableProductDataModel {

    public ToeType ToeType { get; set; } = ToeType.LegLevelers;
    public int LowerAdjShelfQty { get; set; }
    public int UpperAdjShelfQty { get; set; }
    public int LowerVertDivQty { get; set; }
    public int UpperVertDivQty { get; set; }
    public int LowerDoorQty { get; set; }
    public int UpperDoorQty { get; set; }
    public Dimension LowerDoorHeight { get; set; }
    public HingeSide HingeSide { get; set; }
    public bool IsGarage { get; set; }
    public List<string> ProductionNotes { get; set; } = new();
    public Dimension BaseNotchHeight { get; set; }
    public Dimension BaseNotchDepth { get; set; }

    public IProduct MapToProduct() {

        var dbOptions = GetDrawerBoxOptions();
        var mdfConfig = GetMDFDoorConfiguration();

        TallCabinetDoors doors = new() {
            LowerQuantity = LowerDoorQty,
            UpperQuantity = UpperDoorQty,
            LowerDoorHeight = LowerDoorHeight,
            HingeSide = HingeSide
        };

        var rollOuts = GetRollOutOptions();

        var doorConfiguration = GetDoorConfiguration();

        TallCabinetInside inside = new(UpperAdjShelfQty, LowerAdjShelfQty, UpperVertDivQty, LowerVertDivQty, rollOuts);

        var boxMaterial = new CabinetMaterial(BoxMatFinish, BoxFinishType, BoxMatCore);
        var finishMaterial = new CabinetFinishMaterial(FinishMatFinish, FinishFinishType, FinishMatCore, FinishMatPaint);

        CabinetBaseNotch? baseNotch = null;
        if (BaseNotchDepth != Dimension.Zero && BaseNotchHeight != Dimension.Zero) {
            baseNotch = new(BaseNotchHeight, BaseNotchDepth);
        }

        return new TallCabinet(Id, Qty, UnitPrice, ProductNumber, Room, Assembled, Height, Width, Depth, boxMaterial, finishMaterial, doorConfiguration, EdgeBandColor, RightSideType, LeftSideType, Comment,
            doors, ToeType, inside, dbOptions, baseNotch) {
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

           	    tall_cabinets.toe_type AS ToeType,
           	    tall_cabinets.lower_adj_shelf_qty AS LowerAdjShelfQty,
                tall_cabinets.upper_adj_shelf_qty AS UpperAdjShelfQty,
                tall_cabinets.lower_vert_div_qty AS LowerVertDivQty,
                tall_cabinets.upper_vert_div_qty AS UpperVertDivQty,
                tall_cabinets.lower_door_qty AS LowerDoorQty,
                tall_cabinets.upper_door_qty AS UpperDoorQty,
                tall_cabinets.lower_door_height AS LowerDoorHeight,
                tall_cabinets.hinge_side AS HingeSide,
                tall_cabinets.is_garage AS IsGarage,
                tall_cabinets.base_notch_height AS BaseNotchHeight,
                tall_cabinets.base_notch_depth AS BaseNotchDepth,

           	    db_config.material AS DBMaterial,
           	    db_config.slide_type AS DBSlideType,

           	    tall_cabinets.rollout_positions AS ROPositions,
           	    tall_cabinets.rollout_block_type AS ROBlockType,
           	    tall_cabinets.rollout_scoop_front AS ROScoopFront,

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

            FROM tall_cabinets

               JOIN products ON tall_cabinets.product_id = products.id
               JOIN cabinets ON cabinets.product_id = tall_cabinets.product_id
               LEFT JOIN cabinet_db_configs AS db_config ON tall_cabinets.db_config_id = db_config.id
               LEFT JOIN mdf_door_configs ON cabinets.mdf_config_id = mdf_door_configs.id
               LEFT JOIN cabinet_slab_door_materials ON cabinets.slab_door_material_id = cabinet_slab_door_materials.id

            WHERE
           	    products.order_id = @OrderId;
           """;

}