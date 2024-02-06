using System.Data;

namespace Domain.Orders.Persistance;

public interface IOrderingDbConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
