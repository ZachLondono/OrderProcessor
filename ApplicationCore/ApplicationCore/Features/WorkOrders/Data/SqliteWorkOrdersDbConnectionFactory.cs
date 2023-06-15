using ApplicationCore.Features.Configuration;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Infrastructure.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ApplicationCore.Features.WorkOrders.Data;

internal class SqliteWorkOrdersDbConnectionFactory : IWorkOrdersDbConnectionFactory {

    public const int DB_VERSION = 1;
    private static readonly SemaphoreSlim semaphore = new(1);

    private readonly ILogger<SqliteWorkOrdersDbConnectionFactory> _logger;
    private readonly IConfiguration _configuration;
    private readonly DataFilePaths.GetConfiguration _getConfiguration;

    public SqliteWorkOrdersDbConnectionFactory(ILogger<SqliteWorkOrdersDbConnectionFactory> logger, IConfiguration configuration, DataFilePaths.GetConfiguration getConfiguration) {
        _logger = logger;
        _configuration = configuration;
        _getConfiguration = getConfiguration;
    }

    public async Task<IDbConnection> CreateConnection() {

        await semaphore.WaitAsync();

        var result = await _getConfiguration();
        string? datasource = result?.WorkOrdersDBPath ?? null;

        if (datasource is null) {
            semaphore.Release();
            throw new InvalidOperationException("Could not find companies database data source");
        }

        var builder = new SqliteConnectionStringBuilder {
            DataSource = datasource,
            Pooling = false
        };

        var connection = new SqliteConnection(builder.ConnectionString);

        if (File.Exists(datasource)) {

            int dbVersion = await GetDatabaseVersion(connection);
            if (dbVersion != DB_VERSION) {
                semaphore.Release();
                throw new IncompatibleDatabaseVersion(dbVersion);
            }

        } else {

            await InitilizeDatabase(connection);

        }

        semaphore.Release();

        return connection;


    }

    private async Task InitilizeDatabase(SqliteConnection connection) {

        var schemaPath = _configuration.GetRequiredSection("Schemas").GetValue<string>("WorkOrders");

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
