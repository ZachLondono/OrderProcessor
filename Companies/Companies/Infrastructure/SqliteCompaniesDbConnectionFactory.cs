using Dapper;
using Domain.Infrastructure.Data;
using Domain.Infrastructure.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace Companies.Infrastructure;

public class SqliteCompaniesDbConnectionFactory : ICompaniesDbConnectionFactory {

    public const int DB_VERSION = 1;
    private static readonly SemaphoreSlim semaphore = new(1);

    private readonly IConfiguration _configuration;
    private readonly ILogger<SqliteCompaniesDbConnectionFactory> _logger;
    private readonly string _dataSource;

    public SqliteCompaniesDbConnectionFactory(IConfiguration configuration, ILogger<SqliteCompaniesDbConnectionFactory> logger, IOptionsMonitor<DataFilePaths> options) {
        _configuration = configuration;
        _logger = logger;
        _dataSource = options.CurrentValue.CompaniesDBPath;
    }

    public async Task<IDbConnection> CreateConnection() {

        try {

            await semaphore.WaitAsync();

            if (_dataSource is null) {
                throw new InvalidOperationException("Could not find companies database data source");
            }

            var builder = new SqliteConnectionStringBuilder {
                DataSource = _dataSource,
                Pooling = false
            };

            var connection = new SqliteConnection(builder.ConnectionString);

            if (File.Exists(_dataSource)) {

                int dbVersion = await GetDatabaseVersionAsync(connection);
                if (dbVersion != DB_VERSION) {
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
        var relativeSchemaPath = _configuration.GetRequiredSection("Schemas").GetValue<string>("Companies");

        if (relativeSchemaPath is null) {
            connection.Close();
            throw new InvalidOperationException("Companies data base schema path is not set");
        }

        _logger.LogInformation("Initializing companies database, version {DB_VERSION} from schema in file {FilePath}", DB_VERSION, relativeSchemaPath);

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

    private static async Task<int> GetDatabaseVersionAsync(SqliteConnection connection) {

        return await Task.Run(() => connection.QuerySingle<int>("PRAGMA schema_version;"));

    }

}