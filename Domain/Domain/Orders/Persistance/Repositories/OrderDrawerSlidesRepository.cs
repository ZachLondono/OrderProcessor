using Domain.Orders.Entities.Hardware;
using System.Data;
using Domain.ValueObjects;
using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance.Repositories;

public class OrderDrawerSlidesRepository(ISynchronousDbConnection connection, ISynchronousDbTransaction? trx = null) {

    private readonly ISynchronousDbConnection _connection = connection;
    private readonly ISynchronousDbTransaction? _trx = trx;

    public IEnumerable<DrawerSlide> GetOrderDrawerSlides(Guid orderId) {

        var data = _connection.Query<DrawerSlideModel>(
            """
            SELECT
                id,
                qty,
                style,
                length
            FROM drawer_slides 
            WHERE order_id = @OrderId;
            """,
            new {
                OrderId = orderId,
            },
            _trx);

        return data.Select(d => new DrawerSlide(d.Id, d.Qty, d.Length, d.Style));

    }

    public bool AddDrawerSlideToOrder(Guid orderId, DrawerSlide drawerSlide) {

        int rows = _connection.Execute(
            """
            INSERT INTO drawer_slides 
                (id,
                order_id,
                qty,
                style,
                length)
            VALUES
                (@Id,
                @OrderId,
                @Qty,
                @Style,
                @Length);
            """,
            new {
                OrderId = orderId,
                Id = drawerSlide.Id,
                Qty = drawerSlide.Qty,
                Style = drawerSlide.Style,
                Length = drawerSlide.Length
            },
            _trx);

        return rows > 0;
    }

    public bool DeleteDrawerSlide(Guid drawerSlideId) {

        int rows = _connection.Execute(
            """
            DELETE FROM drawer_slides WHERE id = @Id;
            """,
            new {
                Id = drawerSlideId,
            },
            _trx);

        return rows > 0;

    }

    public bool UpdateDrawerSlide(DrawerSlide drawerSlide) {

        int rows = _connection.Execute(
            """
            UPDATE drawer_slides
            SET qty = @Qty, length = @Length, style = @Style
            WHERE id = @Id;
            """,
            drawerSlide,
            _trx);

        return rows > 0;

    }

    private class DrawerSlideModel {
        
        public Guid Id { get; set; }
        public int Qty { get; set; }
        public string Style { get; set; } = string.Empty;
        public Dimension Length { get; set; }

    }

}

