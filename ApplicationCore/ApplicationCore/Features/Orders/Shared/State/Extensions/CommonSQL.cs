using ApplicationCore.Features.Orders.Shared.Domain.Components;
using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.Products.Cabinets;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Shared.State;

public partial class InsertOrder {
    public partial class Handler {

        private static async Task InsertIntoProductTable(IProduct product, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            var parameters = new {
                product.Id,
                OrderId = orderId,
                Qty = product.Qty,
                product.UnitPrice,
                product.ProductNumber,
                Room = product.Room,
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO products
                    (id, order_id, qty, unit_price, product_number, room)
                VALUES
                    (@Id, @OrderId, @Qty, @UnitPrice, @ProductNumber, @Room);
                """, parameters, trx);

        }

        private static async Task InsertCabinet(Cabinet cabinet, Guid? mdfConfigId, IDbConnection connection, IDbTransaction trx) {

            var parameters = new {
                ProductId = cabinet.Id,
                CabHeight = cabinet.Height,
                CabWidth = cabinet.Width,
                CabDepth = cabinet.Depth,
                CabBoxMaterialCore = cabinet.BoxMaterial.Core,
                CabBoxMaterialFinish = cabinet.BoxMaterial.Finish,
                CabBoxMaterialFinishType = cabinet.BoxMaterial.FinishType,
                CabFinishMaterialCore = cabinet.FinishMaterial.Core,
                CabFinishMaterialFinish = cabinet.FinishMaterial.Finish,
                CabFinishMaterialFinishType = cabinet.FinishMaterial.FinishType,
                CabFinishMaterialPaint = cabinet.FinishMaterial.PaintColor,
                SlabDoorCore = cabinet.SlabDoorMaterial?.Core ?? null,
                SlabDoorFinish = cabinet.SlabDoorMaterial?.Finish ?? null,
                SlabDoorFinishType = cabinet.SlabDoorMaterial?.FinishType ?? null,
                SlabDoorPaint = cabinet.SlabDoorMaterial?.PaintColor ?? null,
                CabEdgeBandingFinish = cabinet.EdgeBandingColor,
                CabLeftSideType = cabinet.LeftSideType,
                CabRightSideType = cabinet.RightSideType,
                CabAssembled = cabinet.Assembled,
                CabComment = cabinet.Comment,
                MDFConfigId = mdfConfigId
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO cabinets
                    (product_id,
                    height,
                    width,
                    depth,
                    box_material_core,
                    box_material_finish,
                    box_material_finish_type,
                    finish_material_core,
                    finish_material_finish,
                    finish_material_finish_type,
                    finish_material_paint,
                    slab_door_core,
                    slab_door_finish,
                    slab_door_finish_type,
                    slab_door_paint,
                    edge_banding_finish,
                    left_side_type,
                    right_side_type,
                    assembled,
                    comment,
                    mdf_config_id)
                VALUES
                    (@ProductId,
                    @CabHeight,
                    @CabWidth,
                    @CabDepth,
                    @CabBoxMaterialCore,
                    @CabBoxMaterialFinish,
                    @CabBoxMaterialFinishType,
                    @CabFinishMaterialCore,
                    @CabFinishMaterialFinish,
                    @CabFinishMaterialFinishType,
                    @CabFinishMaterialPaint,
                    @SlabDoorCore,
                    @SlabDoorFinish,
                    @SlabDoorFinishType,
                    @SlabDoorPaint,
                    @CabEdgeBandingFinish,
                    @CabLeftSideType,
                    @CabRightSideType,
                    @CabAssembled,
                    @CabComment,
                    @MDFConfigId);
                """, parameters, trx);

        }

        private static async Task InsertMDFConfig(Guid id, MDFDoorOptions options, IDbConnection connection, IDbTransaction trx) {

            var parameters = new {
                MDFConfigId = id,
                MDFFramingBead = options.FramingBead,
                MDFEdgeDetail = options.EdgeDetail,
                MDFPanelDetail = options.PanelDetail,
                MDFThickness = options.Thickness,
                MDFMaterial = options.Material,
                MDFPanelDrop = options.PanelDrop,
                MDFPaintColor = options.PaintColor
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO mdf_door_configs
                    (id,
                    framing_bead,
                    edge_detail,
                    panel_detail,
                    thickness,
                    material,
                    panel_drop,
                    paint_color)
                VALUES
                    (@MDFConfigId,
                    @MDFFramingBead,
                    @MDFEdgeDetail,
                    @MDFPanelDetail,
                    @MDFThickness,
                    @MDFMaterial,
                    @MDFPanelDrop,
                    @MDFPaintColor);
                """, parameters, trx);

        }

        private static async Task InsertFivePieceDoor(Guid id, FivePieceDoor door, IDbConnection connection, IDbTransaction trx) {

            var parameters = new {
                Id = id,
                door.Width,
                door.Height,
                door.FrameSize.TopRail,
                door.FrameSize.BottomRail,
                door.FrameSize.LeftStile,
                door.FrameSize.RightStile,
                door.FrameThickness,
                door.PanelThickness,
                door.Material
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO five_piece_doors
                    (id,
                    width,
                    height,
                    top_rail,
                    bottom_rail,
                    left_stile,
                    right_stile,
                    frame_thickness,
                    panel_thickness,
                    material)
                VALUES
                    (@Id,
                    @Width,
                    @Height,
                    @TopRail,
                    @BottomRail,
                    @LeftStile,
                    @RightStile,
                    @FrameThickness,
                    @PanelThickness,
                    @Material);
                """, parameters, trx);

        }

        private static async Task InsertCabinetDBConfig(Guid id, CabinetDrawerBoxOptions dbOptions, IDbConnection connection, IDbTransaction trx) {

            var parameters = new {
                Id = id,
                dbOptions.Material,
                dbOptions.SlideType
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO cabinet_db_configs
                    (id,
                    material,
                    slide_type)
                VALUES
                    (@Id,
                    @Material,
                    @SlideType);
                """, parameters, trx);

        }

        private static async Task InsertDBConfig(Guid id, DrawerBoxOptions dbOptions, IDbConnection connection, IDbTransaction trx) {

            var parameters = new {
                Id = id,
                dbOptions.FrontMaterial,
                dbOptions.BackMaterial,
                dbOptions.SideMaterial,
                dbOptions.BottomMaterial,
                dbOptions.Clips,
                dbOptions.Notches,
                dbOptions.Accessory,
                dbOptions.Logo,
                dbOptions.PostFinish,
                dbOptions.ScoopFront,
                dbOptions.FaceMountingHoles,
                dbOptions.Assembled,
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO drawer_box_configs
                    (id,
                    front_material,
                    back_material,
                    side_material,
                    bottom_material,
                    clips,
                    notches,
                    accessory,
                    logo,
                    post_finish,
                    scoop_front,
                    face_mounting_holes,
                    assembled)
                VALUES
                    (@Id,
                    @FrontMaterial,
                    @BackMaterial,
                    @SideMaterial,
                    @BottomMaterial,
                    @Clips,
                    @Notches,
                    @Accessory,
                    @Logo,
                    @PostFinish,
                    @ScoopFront,
                    @FaceMountingHoles,
                    @Assembled);
                """, parameters, trx);

        }

        private static async Task InsertDoweledDB(Guid id, DoweledDrawerBox drawerBox, IDbConnection connection, IDbTransaction trx) {

            var parameters = new {
                Id = id,
                FrontMatName = drawerBox.FrontMaterial.Name,
                FrontMatThickness = drawerBox.FrontMaterial.Thickness,
                FrontMatGraining = drawerBox.FrontMaterial.IsGrained,
                BackMatName = drawerBox.BackMaterial.Name,
                BackMatThickness = drawerBox.BackMaterial.Thickness,
                BackMatGraining = drawerBox.BackMaterial.IsGrained,
                SideMatName = drawerBox.SideMaterial.Name,
                SideMatThickness = drawerBox.SideMaterial.Thickness,
                SideMatGraining = drawerBox.SideMaterial.IsGrained,
                BottomMatName = drawerBox.BottomMaterial.Name,
                BottomMatThickness = drawerBox.BottomMaterial.Thickness,
                BottomMatGraining = drawerBox.BottomMaterial.IsGrained,
                MachineThicknessForUM = drawerBox.MachineThicknessForUMSlides,
                FrontBackHeightAdjustment = drawerBox.FrontBackHeightAdjustment
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO doweled_drawer_box_configs
                    (id,
                    front_mat_name,
                    front_mat_thickness,
                    front_mat_graining,
                    back_mat_name,
                    back_mat_thickness,
                    back_mat_graining,
                    side_mat_name,
                    side_mat_thickness,
                    side_mat_graining,
                    bottom_mat_name,
                    bottom_mat_thickness,
                    bottom_mat_graining,
                    machine_thickness_for_um,
                    frontback_height_adjustment)
                VALUES
                    (@Id,
                    @FrontMatName,
                    @FrontMatThickness,
                    @FrontMatGraining,
                    @BackMatName,
                    @BackMatThickness,
                    @BackMatGraining,
                    @SideMatName,
                    @SideMatThickness,
                    @SideMatGraining,
                    @BottomMatName,
                    @BottomMatThickness,
                    @BottomMatGraining,
                    @MachineThicknessForUM,
                    @FrontBackHeightAdjustment); 
                """, parameters, trx);

        }
    }
}