using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ApplicationCore.Features.Orders.Data;

internal class SqliteOrderingDbConnectionFactory : IOrderingDbConnectionFactory {

    public const int DB_VERSION = 1;

    private readonly IConfiguration _configuration;

    public SqliteOrderingDbConnectionFactory(IConfiguration configuration) {
        _configuration = configuration;
    }

    public async Task<IDbConnection> CreateConnection() {

        var datasource = _configuration.GetRequiredSection("Ordering").GetValue<string>("Data Source");

        bool doesExist = File.Exists(datasource);

        var builder = new SqliteConnectionStringBuilder {
            DataSource = datasource,
            Pooling = false
        };

        var connection = new SqliteConnection(builder.ConnectionString);

        if (doesExist ) { 

            int dbVersion = await GetDatabaseVersion(connection);
            if (dbVersion != DB_VERSION) {
                throw new IncompatibleDatabaseVersion(dbVersion);
            }

        } else {

            await InitilizeDatabase(connection);

        }

        return connection;

    }

    private async Task InitilizeDatabase(SqliteConnection connection) {

        var schemaPath = _configuration.GetRequiredSection("Ordering").GetValue<string>("Schema");

        if (schemaPath is null) {
            throw new InvalidOperationException("Ordering data base schema path is not set");
        }

        var schema = await File.ReadAllTextAsync(schemaPath);
        
        await connection.ExecuteAsync(schema);

        await connection.ExecuteAsync($"PRAGMA SCHEMA_VERSION = {DB_VERSION};");
        
    }

    private async Task<int> GetDatabaseVersion(SqliteConnection connection) {
        return await connection.QuerySingleAsync<int>("PRAGMA schema_version;");
    }

}
