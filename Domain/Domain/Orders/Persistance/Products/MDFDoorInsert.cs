using Domain.Infrastructure.Data;
using Domain.Orders.Entities.Products.Doors;
using Domain.Orders.ValueObjects;

namespace Domain.Orders.Persistance.Products;

public static partial class ProductsPersistance {

    public static void InsertProduct(MDFDoorProduct mdfdoor, Guid orderId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

        InsertMDFConfig(mdfdoor.Id, mdfdoor.GetMDFDoorOptions(), connection, trx);
        InsertIntoProductTable(mdfdoor, orderId, connection, trx);
        var (isOpenPanel, panelId) = InsertMDFPanel(mdfdoor.Panel, connection, trx);

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
            PanelId = (Guid?) (isOpenPanel ? panelId : null),
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
                        mdf_open_panel_id)
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
                        @PanelId);
                    """, parameters, trx);

        if (mdfdoor.AdditionalOpenings.Any()) {
            foreach (var opening in mdfdoor.AdditionalOpenings) {

                (bool isOpen, Guid openingId) = InsertMDFPanel(opening.Panel, connection, trx);

                connection.Execute(
                    """
                    INSERT INTO mdf_door_openings 
                        (id,
                        product_id,
                        opening,
                        rail,
                        mdf_open_panel_id)
                    VALUES
                        (@Id,
                        @ProductId,
                        @Opening,
                        @Rail,
                        @PanelId);
                    """, new {
                        Id = Guid.NewGuid(),
                        ProductId = mdfdoor.Id,
                        Opening = opening.OpeningHeight,
                        Rail = opening.RailWidth,
                        PanelId = (Guid?) (isOpen ? openingId : null),
                    }, trx);

            }
        }

    }

    public static (bool, Guid) InsertMDFPanel(MDFDoorPanel panel, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {

        Guid panelId = Guid.NewGuid();

        return panel.Match(
            (SolidPanel s) => (false, panelId),
            (OpenPanel o) => {

                connection.Execute("""
                INSERT INTO mdf_panels
                    (id,
                    name,
                    rabbet_back,
                    route_for_gasket)
                VALUES
                    (@Id,
                    @Name,
                    @RabbetBack,
                    @RouteForGasket);
                """, new {
                    Id = panelId,
                    RabbetBack = o.RabbetBack,
                    RouteForGasket = o.RouteForGasket
                }, trx);
                return (true, panelId);

            });


    }

}