using ApplicationCore.Features.Configuration;
using ApplicationCore.Infrastructure.Bus;
using Microsoft.Data.Sqlite;
using System.Data;

namespace ApplicationCore.Features.WorkOrders.Data;

internal class SqliteWorkOrdersDbConnectionFactory : IWorkOrdersDbConnectionFactory {

    private readonly IBus _bus;

    public SqliteWorkOrdersDbConnectionFactory(IBus bus) {
        _bus = bus;
    }

    public async Task<IDbConnection> CreateConnection() {

        var result = await _bus.Send(new GetConfiguration.Query());
        string? datasource = null;
        result.OnSuccess(
            appConfig => datasource = appConfig.WorkOrdersDBPath
        );

        var builder = new SqliteConnectionStringBuilder {
            DataSource = datasource,
            Pooling = false
        };

        var connection = new SqliteConnection(builder.ConnectionString);

        return connection;

    }

}
