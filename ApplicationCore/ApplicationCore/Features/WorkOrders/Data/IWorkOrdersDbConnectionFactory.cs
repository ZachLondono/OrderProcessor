using System.Data;

namespace ApplicationCore.Features.WorkOrders.Data;

public interface IWorkOrdersDbConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
