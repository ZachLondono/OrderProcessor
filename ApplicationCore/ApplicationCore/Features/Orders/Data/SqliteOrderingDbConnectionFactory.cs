using ApplicationCore.Features.Configuration;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ApplicationCore.Features.Orders.Data;

internal class SqliteOrderingDbConnectionFactory : IOrderingDbConnectionFactory {

    public const int DB_VERSION = 1;
    private static readonly SemaphoreSlim semaphore = new(1);

    private readonly IConfiguration _configuration;
    private readonly IBus _bus;
    private readonly ILogger<SqliteOrderingDbConnectionFactory> _logger;

    public SqliteOrderingDbConnectionFactory(IConfiguration configuration, IBus bus, ILogger<SqliteOrderingDbConnectionFactory> logger) {
        _configuration = configuration;
        _bus = bus;
        _logger = logger;
    }

    public async Task<IDbConnection> CreateConnection() {

        await semaphore.WaitAsync();

        var result = await _bus.Send(new GetConfiguration.Query());
        string? datasource = null;
        result.OnSuccess(
            appConfig => datasource = appConfig.OrderingDBPath
        );

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

        semaphore.Release();

        return connection;

    }

    private async Task InitilizeDatabase(SqliteConnection connection) {

        var schemaPath = _configuration.GetRequiredSection("Ordering").GetValue<string>("Schema");

        if (schemaPath is null) {
            throw new InvalidOperationException("Ordering data base schema path is not set");
        }

        _logger.LogInformation("Initilizing ordering database, version {DB_VERSION} from schema in file {FilePath}", DB_VERSION, schemaPath);

        var schema = await File.ReadAllTextAsync(schemaPath);

        await connection.OpenAsync();
        var trx = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(schema, trx);
        await connection.ExecuteAsync($"PRAGMA SCHEMA_VERSION = {DB_VERSION};", trx);

        trx.Commit();
        connection.Close();

    }

    private async Task<int> GetDatabaseVersion(SqliteConnection connection) {
        return await connection.QuerySingleAsync<int>("PRAGMA schema_version;");
    }

}
