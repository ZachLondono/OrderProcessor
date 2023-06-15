using System.Data;

namespace ApplicationCore.Shared.Data.Ordering;

public interface IOrderingDbConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
