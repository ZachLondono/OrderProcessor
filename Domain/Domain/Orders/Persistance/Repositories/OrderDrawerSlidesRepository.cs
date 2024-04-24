using Domain.Orders.Entities.Hardware;
using Dapper;
using System.Data;
using Domain.ValueObjects;

namespace Domain.Orders.Persistance.Repositories;

public class OrderDrawerSlidesRepository(IDbConnection connection, IDbTransaction? trx = null) {

    private readonly IDbConnection _connection = connection;
    private readonly IDbTransaction? _trx = trx;

    public async Task<IEnumerable<DrawerSlide>> GetOrderDrawerSlides(Guid orderId) {

        var data = await _connection.QueryAsync<DrawerSlideModel>(
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

    public async Task<bool> AddDrawerSlideToOrder(Guid orderId, DrawerSlide drawerSlide) {

        int rows = await _connection.ExecuteAsync(
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

    public async Task<bool> DeleteDrawerSlide(Guid drawerSlideId) {

        int rows = await _connection.ExecuteAsync(
            """
            DELETE FROM drawer_slides WHERE id = @Id;
            """,
            new {
                Id = drawerSlideId,
            },
            _trx);

        return rows > 0;

    }

    public async Task<bool> UpdateDrawerSlide(DrawerSlide drawerSlide) {

        int rows = await _connection.ExecuteAsync(
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

