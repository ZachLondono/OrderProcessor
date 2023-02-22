using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ApplicationCore.Infrastructure.Data;

internal class SqliteDbConnectionFactory : IDbConnectionFactory {

    private readonly IConfiguration _configuration;

    public SqliteDbConnectionFactory(IConfiguration configuration) {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection() {

        var connstring = _configuration.GetConnectionString("DrawerBoxOrders");
        var connection = new SqliteConnection(connstring);

        return connection;

    }

}
