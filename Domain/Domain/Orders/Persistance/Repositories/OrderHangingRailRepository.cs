using Domain.Infrastructure.Data;
using Domain.Orders.Entities.Hardware;
using Domain.ValueObjects;
using System.Data;

namespace Domain.Orders.Persistance.Repositories;

public class OrderHangingRailRepository(ISynchronousDbConnection connection, ISynchronousDbTransaction? trx = null) {

    private readonly ISynchronousDbConnection _connection = connection;
    private readonly ISynchronousDbTransaction? _trx = trx;

    public async Task<IEnumerable<HangingRail>> GetOrderHangingRails(Guid orderId) {

        var data = _connection.Query<HangingRailModel>(
            """
            SELECT
                id,
                qty,
                finish,
                length
            FROM hanging_rails
            WHERE order_id = @OrderId;
            """,
            new {
                OrderId = orderId,
            },
            _trx);

        return data.Select(d => new HangingRail(d.Id, d.Qty, d.Length, d.Finish));

    }

    public async Task<bool> AddHangingRailToOrder(Guid orderId, HangingRail hangingRail) {

        int rows = _connection.Execute(
            """
            INSERT INTO hanging_rails
                (id,
                order_id,
                qty,
                finish,
                length)
            VALUES
                (@Id,
                @OrderId,
                @Qty,
                @Finish,
                @Length);
            """,
            new {
                OrderId = orderId,
                Id = hangingRail.Id,
                Qty = hangingRail.Qty,
                Finish = hangingRail.Finish,
                Length = hangingRail.Length
            },
            _trx);

        return rows > 0;

    }

    public async Task<bool> DeleteHangingRail(Guid hangingRailId) {

        int rows = _connection.Execute(
            """
            DELETE FROM hanging_rails WHERE id = @Id;
            """,
            new {
                Id = hangingRailId,
            },
            _trx);

        return rows > 0;

    }

    public async Task<bool> UpdateHangingRail(HangingRail hangingRail) {

        int rows = _connection.Execute(
            """
            UPDATE hanging_rails
            SET qty = @Qty, length = @Length, finish = @Finish
            WHERE id = @Id;
            """,
            hangingRail,
            _trx);

        return rows > 0;

    }

    private class HangingRailModel {

        public Guid Id { get; set; }
        public int Qty { get; set; }
        public Dimension Length { get; set; }
        public string Finish { get; set; } = string.Empty;

    }

}

