using System.Data;

namespace ApplicationCore.Features.Configuration;

internal interface IConfigurationDBConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
