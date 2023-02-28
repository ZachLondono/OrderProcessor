using System.Data;

namespace ApplicationCore.Features.Orders.Data;

public interface IOrderingDbConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
