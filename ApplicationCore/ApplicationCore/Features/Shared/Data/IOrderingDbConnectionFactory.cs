using System.Data;

namespace ApplicationCore.Features.Shared.Data;

internal interface IOrderingDbConnectionFactory {

    public IDbConnection CreateConnection();

}
