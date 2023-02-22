using ApplicationCore.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ApplicationCore.Features.Shared.Data;

internal class SqliteOrderingDbConnectionFactory : IDbConnectionFactory {

    private readonly IConfiguration _configuration;

    public SqliteOrderingDbConnectionFactory(IConfiguration configuration) {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection() {

        var connstring = _configuration.GetConnectionString("Ordering");
        var connection = new SqliteConnection(connstring);

        return connection;

    }

}
