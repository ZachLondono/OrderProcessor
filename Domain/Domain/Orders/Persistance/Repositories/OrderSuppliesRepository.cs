using Domain.Infrastructure.Data;
using Domain.Orders.Entities.Hardware;
using System.Data;

namespace Domain.Orders.Persistance.Repositories;

public class OrderSuppliesRepository(ISynchronousDbConnection connection, ISynchronousDbTransaction? trx = null) {

    private readonly ISynchronousDbConnection _connection = connection;
    private readonly ISynchronousDbTransaction? _trx = trx;

    public IEnumerable<Supply> GetOrderSupplies(Guid orderId) {

        var data = _connection.Query<SupplyModel>(
            """
            SELECT
                id,
                qty,
                description
            FROM supplies
            WHERE order_id = @OrderId;
            """,
            new {
                OrderId = orderId,
            },
            _trx);

        return data.Select(d => new Supply(d.Id, d.Qty, d.Description));

    }

    public bool AddSupplyToOrder(Guid orderId, Supply supply) {

        var rows = _connection.Execute(
            """
            INSERT INTO supplies
                (id,
                order_id,
                qty,
                description)
            VALUES
                (@Id,
                @OrderId,
                @Qty,
                @Description);
            """,
            new {
                OrderId = orderId,
                Id = supply.Id,
                Qty = supply.Qty,
                Description = supply.Description
            },
            _trx);

        return rows > 0;

    }

    public bool DeleteSupply(Guid supplyId) {
        
        var rows = _connection.Execute(
            """
            DELETE FROM supplies
            WHERE id = @Id;
            """,
            new {
                Id = supplyId,
            },
            _trx);

        return rows > 0;

    }

    public bool UpdateSupply(Supply supply) {

        var rows = _connection.Execute(
            """
            UPDATE supplies
            SET qty = @Qty, description = @Description
            WHERE id = @Id;
            """,
            supply,
            _trx);

        return rows > 0;

    }

    private class SupplyModel {

        public Guid Id { get; set; }
        public int Qty { get; set; }
        public string Description { get; set; } = string.Empty;

    }

}
