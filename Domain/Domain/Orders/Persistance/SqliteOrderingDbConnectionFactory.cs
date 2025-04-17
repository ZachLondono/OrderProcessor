using Dapper;
using Domain.Infrastructure.Data;
using Domain.Infrastructure.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Domain.Orders.Persistance;

public class SqliteOrderingDbConnectionFactory : IOrderingDbConnectionFactory {

    public const int DB_VERSION = 8;
    private static readonly SemaphoreSlim semaphore = new(1);

    private readonly IConfiguration _configuration;
    private readonly ILogger<SqliteOrderingDbConnectionFactory> _logger;
    private readonly string _dataSource;

    public SqliteOrderingDbConnectionFactory(IConfiguration configuration, ILogger<SqliteOrderingDbConnectionFactory> logger, IOptionsMonitor<DataFilePaths> options) {
        _configuration = configuration;
        _logger = logger;
        _dataSource = options.CurrentValue.OrderingDBPath;
    }

    public Task<ISynchronousDbConnection> CreateConnection() => CreateConnection(_dataSource);

    public async Task<ISynchronousDbConnection> CreateConnection(string dataSource) {

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

                int dbVersion = GetDatabaseVersion(connection);
                if (dbVersion != DB_VERSION) {
                    semaphore.Release();
                    throw new IncompatibleDatabaseVersion(dbVersion, DB_VERSION);
                }

            } else {

                try {

                    var dir = Path.GetDirectoryName(_dataSource);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                        var info = Directory.CreateDirectory(dir);
                        if (!info.Exists) {
                            throw new InvalidDataException($"Failed to create data directory for data source '{dataSource}'");
                        }
                    }

                    await InitializeDatabase(connection);

                } catch (Exception ex) {
                    throw new DataBaseInitializationException(ex);
                }

            }

            return new SynchronousSQLiteDbConnection(connection);

        } catch (Exception ex) {

            _logger.LogError(ex, "Exception thrown while creating database connection");

            throw;

        } finally {

            semaphore.Release();

        }

    }

    private async Task InitializeDatabase(SqliteConnection connection) {

        string? directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        if (directory is null) {
            throw new InvalidOperationException("Failed to get directory of executing assembly. Could not find path to ordering database schema.");
        }

        var relativeSchemaPath = _configuration.GetRequiredSection("Schemas").GetValue<string>("Ordering");

        if (relativeSchemaPath is null) {
            throw new InvalidOperationException("Ordering data base schema path is not set");
        }

        _logger.LogInformation("Initializing ordering database, version {DB_VERSION} from schema in file {FilePath}", DB_VERSION, relativeSchemaPath);

        string fullPath = Path.Combine(directory, relativeSchemaPath);
        var schema = await File.ReadAllTextAsync(fullPath);

        await Task.Run(() => {

            connection.Open();
            var trx = connection.BeginTransaction();

            connection.Execute(schema, trx);
            connection.Execute($"PRAGMA SCHEMA_VERSION = {DB_VERSION};", trx);

            trx.Commit();
            connection.Close();

        });

    }

    private static int GetDatabaseVersion(SqliteConnection connection) {
        return connection.QuerySingle<int>("PRAGMA schema_version;");
    }

}
