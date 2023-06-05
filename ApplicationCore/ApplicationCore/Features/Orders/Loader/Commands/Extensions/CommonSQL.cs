using ApplicationCore.Features.Orders.Shared.Domain.Products;
using ApplicationCore.Features.Orders.Shared.Domain.ValueObjects;
using Dapper;
using System.Data;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertIntoProductTable(IProduct product, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            var parameters = new {
                product.Id,
                OrderId = orderId,
                Qty = product.Qty,
                product.UnitPrice,
                product.ProductNumber,
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO products
                    (id, order_id, qty, unit_price, product_number)
                VALUES
                    (@Id, @OrderId, @Qty, @UnitPrice, @ProductNumber);
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
                CabFinishMaterialCore = cabinet.FinishMaterial.Core,
                CabFinishMaterialFinish = cabinet.FinishMaterial.Finish,
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
                CabRoom = cabinet.Room,
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
                    finish_material_core,
                    finish_material_finish,
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
                    room,
                    mdf_config_id)
                VALUES
                    (@ProductId,
                    @CabHeight,
                    @CabWidth,
                    @CabDepth,
                    @CabBoxMaterialCore,
                    @CabBoxMaterialFinish,
                    @CabFinishMaterialCore,
                    @CabFinishMaterialFinish,
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
                    @CabRoom,
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

    }
}