using System.Data;

namespace ApplicationCore.Features.Shared.Data.Ordering;

public interface IOrderingDbConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
