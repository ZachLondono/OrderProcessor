using Dapper;
using Domain.Infrastructure.Data;
using Domain.Infrastructure.Settings;
using Domain.Orders.Persistance;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace Domain.Orders.Persistance;

public class SqliteOrderingDbConnectionFactory : IOrderingDbConnectionFactory {

    public const int DB_VERSION = 1;
    private static readonly SemaphoreSlim semaphore = new(1);

    private readonly IConfiguration _configuration;
    private readonly ILogger<SqliteOrderingDbConnectionFactory> _logger;
    private readonly string _dataSource;

    public SqliteOrderingDbConnectionFactory(IConfiguration configuration, ILogger<SqliteOrderingDbConnectionFactory> logger, IOptionsMonitor<DataFilePaths> options) {
        _configuration = configuration;
        _logger = logger;
        _dataSource = options.CurrentValue.OrderingDBPath;
    }

    public Task<IDbConnection> CreateConnection() => CreateConnection(_dataSource);

    public async Task<IDbConnection> CreateConnection(string dataSource) {

        try {

            await semaphore.WaitAsync();

            if (dataSource is null) {
                throw new InvalidOperationException("Could not find ordering database data source");
            }

            var builder = new SqliteConnectionStringBuilder {
                DataSource = dataSource,
                Pooling = false
            };

            var connection = new SqliteConnection(builder.ConnectionString);

            if (File.Exists(dataSource)) {

                int dbVersion = await GetDatabaseVersion(connection);
                if (dbVersion != DB_VERSION) {
                    semaphore.Release();
                    throw new IncompatibleDatabaseVersion(dbVersion);
                }

            } else {

                try {
                    await InitializeDatabase(connection);
                } catch (Exception ex) {
                    throw new DataBaseInitializationException(ex);
                }

            }

            return connection;

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while creating database connection");

            throw;

        } finally {

            semaphore.Release();

        }

    }

    private async Task InitializeDatabase(SqliteConnection connection) {

        var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var relativeSchemaPath = _configuration.GetRequiredSection("Schemas").GetValue<string>("Ordering");

        if (relativeSchemaPath is null) {
            throw new InvalidOperationException("Ordering data base schema path is not set");
        }

        _logger.LogInformation("Initializing ordering database, version {DB_VERSION} from schema in file {FilePath}", DB_VERSION, relativeSchemaPath);

        string fullPath = Path.Combine(directory, relativeSchemaPath);
        var schema = await File.ReadAllTextAsync(fullPath);

        await connection.OpenAsync();
        var trx = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(schema, trx);
        await connection.ExecuteAsync($"PRAGMA SCHEMA_VERSION = {DB_VERSION};", trx);

        trx.Commit();
        connection.Close();

    }

    private static async Task<int> GetDatabaseVersion(SqliteConnection connection) {
        return await connection.QuerySingleAsync<int>("PRAGMA schema_version;");
    }

}
