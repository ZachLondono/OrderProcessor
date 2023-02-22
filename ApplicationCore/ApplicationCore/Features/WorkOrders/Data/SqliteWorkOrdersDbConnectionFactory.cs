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

        var connstring = _configuration.GetConnectionString("WorkOrders");
        var connection = new SqliteConnection(connstring);

        return connection;

    }

}
