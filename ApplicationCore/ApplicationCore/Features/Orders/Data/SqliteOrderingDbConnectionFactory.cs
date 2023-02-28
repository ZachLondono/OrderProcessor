using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ApplicationCore.Features.Orders.Data;

internal class SqliteOrderingDbConnectionFactory : IOrderingDbConnectionFactory {

    public const int DB_VERSION = 1;

    private readonly IConfiguration _configuration;
    private readonly ILogger<SqliteOrderingDbConnectionFactory> _logger;

    public SqliteOrderingDbConnectionFactory(IConfiguration configuration, ILogger<SqliteOrderingDbConnectionFactory> logger) {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IDbConnection> CreateConnection() {

        var datasource = _configuration.GetRequiredSection("Ordering").GetValue<string>("Data Source");

        var builder = new SqliteConnectionStringBuilder {
            DataSource = datasource,
            Pooling = false
        };

        var connection = new SqliteConnection(builder.ConnectionString);

        if (File.Exists(datasource)) { 

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

        _logger.LogInformation("Initilizing ordering database, version {DB_VERSION} from schema in file {FilePath}", DB_VERSION, schemaPath);

        var schema = await File.ReadAllTextAsync(schemaPath);
        
        await connection.ExecuteAsync(schema);

        await connection.ExecuteAsync($"PRAGMA SCHEMA_VERSION = {DB_VERSION};");
        
    }

    private async Task<int> GetDatabaseVersion(SqliteConnection connection) {
        return await connection.QuerySingleAsync<int>("PRAGMA schema_version;");
    }

}
