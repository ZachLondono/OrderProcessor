using Domain.Infrastructure.Data;
using Domain.Orders.Entities.Products.Doors;

namespace Domain.Orders.Persistance.Products;

public static partial class ProductsPersistance {

    public static void InsertProduct(MDFDoorProduct mdfdoor, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

        InsertMDFConfig(mdfdoor.Id, mdfdoor.GetMDFDoorOptions(), connection, trx);
        InsertIntoProductTable(mdfdoor, orderId, connection, trx);

        var parameters = new {
            ProductId = mdfdoor.Id,
            mdfdoor.Note,
            mdfdoor.Height,
            mdfdoor.Width,
            mdfdoor.Type,
            mdfdoor.FrameSize.TopRail,
            mdfdoor.FrameSize.BottomRail,
            mdfdoor.FrameSize.LeftStile,
            mdfdoor.FrameSize.RightStile,
            mdfdoor.Orientation,
            mdfdoor.IsOpenPanel
        };

        connection.Execute("""
                    INSERT INTO mdf_door_products
                        (product_id,
                        note,
                        height,
                        width,
                        type,
                        top_rail,
                        bottom_rail,
                        left_stile,
                        right_stile,
                        orientation,
                        is_open_panel)
                    VALUES
                        (@ProductId,
                        @Note,
                        @Height,
                        @Width,
                        @Type,
                        @TopRail,
                        @BottomRail,
                        @LeftStile,
                        @RightStile,
                        @Orientation,
                        @IsOpenPanel);
                    """, parameters, trx);

        if (mdfdoor.AdditionalOpenings.Any()) {
            foreach (var opening in mdfdoor.AdditionalOpenings) {
                connection.Execute("""
                        INSERT INTO mdf_door_openings 
                            (id,
                            product_id,
                            opening,
                            rail,
                            is_open_panel)
                        VALUES
                            (@Id,
                            @ProductId,
                            @Opening,
                            @Rail,
                            @IsOpenPanel);
                       """, new {
                    Id = Guid.NewGuid(),
                    ProductId = mdfdoor.Id,
                    Opening = opening.OpeningHeight,
                    Rail = opening.RailWidth,
                    IsOpenPanel = opening.IsOpenPanel
                }, trx);
            }
        }

    }

}