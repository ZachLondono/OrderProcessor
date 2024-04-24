using Dapper;
using Domain.Orders.Entities.Hardware;
using System.Data;

namespace Domain.Orders.Persistance.Repositories;

public class OrderSuppliesRepository(IDbConnection connection, IDbTransaction? trx = null) {

    private readonly IDbConnection _connection = connection;
    private readonly IDbTransaction? _trx = trx;

    public async Task<IEnumerable<Supply>> GetOrderSupplies(Guid orderId) {

        var data = await _connection.QueryAsync<SupplyModel>(
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

    public async Task<bool> AddSupplyToOrder(Guid orderId, Supply supply) {

        var rows = await _connection.ExecuteAsync(
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

    public async Task<bool> DeleteSupply(Guid supplyId) {
        
        var rows = await _connection.ExecuteAsync(
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

    public async Task<bool> UpdateSupply(Supply supply) {

        var rows = await _connection.ExecuteAsync(
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
        public string Description { get; set; }

    }

}
