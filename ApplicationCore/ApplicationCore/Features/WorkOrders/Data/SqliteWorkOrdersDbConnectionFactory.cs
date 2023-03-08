using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ApplicationCore.Features.WorkOrders.Data;

internal class SqliteWorkOrdersDbConnectionFactory : IWorkOrdersDbConnectionFactory {

    private readonly IConfiguration _configuration;

    public SqliteWorkOrdersDbConnectionFactory(IConfiguration configuration) {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection() {

        var datasource = _configuration.GetRequiredSection("WorkOrders").GetValue<string>("Data Source");

        var builder = new SqliteConnectionStringBuilder {
            DataSource = datasource,
            Pooling = false
        };

        var connection = new SqliteConnection(builder.ConnectionString);

        return connection;

    }

}
