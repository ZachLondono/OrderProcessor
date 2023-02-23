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
                CabEdgeBandingFinish = cabinet.EdgeBandingColor,
                CabLeftSideType = cabinet.LeftSide.Type,
                CabRightSideType = cabinet.RightSide.Type,
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
                    edge_bading_finish,
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
                    @CabEdgeBandingFinish,
                    @CabLeftSideType,
                    @CabRightSideType,
                    @CabAssembled,
                    @CabComment,
                    @CabRoom,
                    @MDFConfigId);
                """, parameters, trx);

        }

        private static async Task InsertRollOutConfig(Guid id, RollOutOptions rollout, IDbConnection connection, IDbTransaction trx) {

            var parameters = new {
                ROConfigId = id,
                ROConfigPositions = rollout.Positions,
                ROConfigBlockType = rollout.Blocks,
                ROConfigScoopFront = rollout.ScoopFront
            };

            await connection.ExecuteAsync(
                """
                INSERT INTO roll_out_configs
                    (id, positions, block_type, scoop_front)
                VALUES
                    (@ROConfigId, @ROConfigPositions, @ROConfigBlockType, @ROConfigScoopFront);
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
                SlideType = dbOptions.SlideType,
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
                    assembled,
                    slide_type)
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
                    @Assembled,
                    @SlideType);
                """, parameters, trx);

        }

    }
}