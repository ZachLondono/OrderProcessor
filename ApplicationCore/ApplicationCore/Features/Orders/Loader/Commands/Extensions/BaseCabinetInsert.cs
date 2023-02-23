using ApplicationCore.Features.Orders.Shared.Domain.Products;
using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Data;

namespace ApplicationCore.Features.Orders.Loader.Commands;

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(BaseCabinet cabinet, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            Guid? mdfConfigId = null;
            var mdfConfig = cabinet.MDFDoorOptions;
            if (mdfConfig is not null) {
                mdfConfigId = Guid.NewGuid();
                await InsertMDFConfig((Guid)mdfConfigId, mdfConfig, connection, trx);
            }

            Guid? rollOutConfigId = null;
            if (cabinet.Inside.RollOutBoxes.Any()) {
                rollOutConfigId = Guid.NewGuid();
                await InsertRollOutConfig((Guid)rollOutConfigId, cabinet.Inside.RollOutBoxes, connection, trx);
            }

            var dbConfigId = Guid.NewGuid();
            await InsertDBConfig(dbConfigId, cabinet.DrawerBoxOptions, connection, trx);

            await InsertIntoProductTable(cabinet, orderId, connection, trx);
            await InsertCabinet(cabinet, mdfConfigId, connection, trx);

            var parameters = new {
                ProductId = cabinet.Id,
                ToeType = cabinet.ToeType,
                DoorQty = cabinet.Doors.Quantity,
                HingeSide = cabinet.Doors.HingeSide,
                ROConfigId = rollOutConfigId,
                AdjShelfQty = cabinet.Inside.AdjustableShelves,
                VertDivQty = cabinet.Inside.VerticalDividers,
                ShelfDepth = cabinet.Inside.ShelfDepth,
                DrawerFaceHeight = cabinet.Drawers.FaceHeight,
                DrawerQty = cabinet.Drawers.Quantity,
                DBConfigId = dbConfigId
            };

            await connection.ExecuteAsync("""
                    INSERT INTO base_cabinets
                        (product_id,
                        toe_type,
                        door_qty,
                        hinge_side,
                        roll_out_config_id,
                        adj_shelf_qty,
                        vert_div_qty,
                        shelf_depth,
                        drawer_face_height,
                        drawer_qty,
                        db_config_id)
                    VALUES
                        (@ProductId,
                        @ToeType,
                        @DoorQty,
                        @HingeSide,
                        @ROConfigId,
                        @AdjShelfQty,
                        @VertDivQty,
                        @ShelfDepth,
                        @DrawerFaceHeight,
                        @DrawerQty,
                        @DBConfigId);
                    """, parameters, trx);

        }


    }

}

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(ClosetPart closetPart, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertIntoProductTable(closetPart, orderId, connection, trx);
            
            var parameters = new {
                ProductId = closetPart.Id,
                Room = closetPart.Room,
                Sku = closetPart.SKU,
                Width = closetPart.Width,
                Length = closetPart.Length,
                MaterialFinish = closetPart.Material.Finish,
                MaterialCore = closetPart.Material.Core,
                PaintColor = closetPart.Material.PaintColor,
                EdgeBandingFinish = closetPart.EdgeBandingColor,
                Comment = closetPart.Comment,
                Parameters = (IDictionary<string, string>) closetPart.Parameters
            };

            await connection.ExecuteAsync("""
                    INSERT INTO closet_parts
                        (product_id,
                        room,
                        sku,
                        width,
                        length,
                        material_finish,
                        material_core,
                        paint_color,
                        edge_banding_finish,
                        comment,
                        parameters)
                    VALUES
                        (@ProductId,
                        @Room,
                        @Sku,
                        @Width,
                        @Length,
                        @MaterialFinish,
                        @MaterialCore,
                        @PaintColor,
                        @EdgeBandingFinish,
                        @Comment,
                        @Parameters);
                    """, parameters, trx);

        }

    }

}

public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(MDFDoorProduct mdfdoor, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertMDFConfig(mdfdoor.Id, mdfdoor, connection, trx);
            await InsertIntoProductTable(mdfdoor, orderId, connection, trx);

            var parameters = new {
                ProductId = mdfdoor.Id,
                mdfdoor.Height,
                mdfdoor.Width,
                mdfdoor.Type,
                mdfdoor.FrameSize.TopRail,
                mdfdoor.FrameSize.BottomRail,
                mdfdoor.FrameSize.LeftStile,
                mdfdoor.FrameSize.RightStile,
            };

            await connection.ExecuteAsync("""
                    INSERT INTO mdf_door_products
                        (product_id,
                        height,
                        width,
                        type,
                        top_rail,
                        bottom_rail,
                        left_stile,
                        right_stile)
                    VALUES
                        (@ProductId,
                        @Height,
                        @Width,
                        @Type,
                        @TopRail,
                        @BottomRail,
                        @LeftStile,
                        @RightStile);
                    """, parameters, trx);

        }

    }

}


public partial class CreateNewOrder {
    public partial class Handler {

        private static async Task InsertProduct(DovetailDrawerBoxProduct drawerbox, Guid orderId, IDbConnection connection, IDbTransaction trx) {

            await InsertDBConfig(drawerbox.Id, drawerbox.DrawerBoxOptions, connection, trx);
            await InsertIntoProductTable(drawerbox, orderId, connection, trx);

            var parameters = new {
                ProductId = drawerbox.Id,
                drawerbox.Height,
                drawerbox.Width,
                drawerbox.Depth
            };

            await connection.ExecuteAsync("""
                    INSERT INTO dovetail_door_products
                        (product_id,
                        height,
                        width,
                        depth)
                    VALUES
                        (@ProductId,
                        @Height,
                        @Width,
                        @Depth);
                    """, parameters, trx);

        }

    }

}