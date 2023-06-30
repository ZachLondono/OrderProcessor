using ApplicationCore.Shared.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ApplicationCore.Shared.Data.Companies;

public class SqliteCompaniesDbConnectionFactory : ICompaniesDbConnectionFactory {

    public const int DB_VERSION = 1;
    private static readonly SemaphoreSlim semaphore = new(1);

    private readonly IConfiguration _configuration;
    private readonly ILogger<SqliteCompaniesDbConnectionFactory> _logger;
    private readonly DataFilePaths.GetConfiguration _getConfiguration;

    public SqliteCompaniesDbConnectionFactory(IConfiguration configuration, ILogger<SqliteCompaniesDbConnectionFactory> logger, DataFilePaths.GetConfiguration getConfiguration) {
        _configuration = configuration;
        _logger = logger;
        _getConfiguration = getConfiguration;
    }

    public async Task<IDbConnection> CreateConnection() {

        await semaphore.WaitAsync();

        var result = await _getConfiguration();
        string? datasource = result?.CompaniesDBPath ?? null;

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
                throw new IncompatibleDatabaseVersion(dbVersion);
            }

        } else {

            try {
                await InitializeDatabase(connection);
            } catch (Exception ex) {
                throw new DataBaseInitializationException(ex);
            }

        }

        semaphore.Release();

        return connection;

    }

    private async Task InitializeDatabase(SqliteConnection connection) {

        var schemaPath = _configuration.GetRequiredSection("Schemas").GetValue<string>("Companies");

        if (schemaPath is null) {
            connection.Close();
            throw new InvalidOperationException("Companies data base schema path is not set");
        }

        _logger.LogInformation("Initializing companies database, version {DB_VERSION} from schema in file {FilePath}", DB_VERSION, schemaPath);

        var schema = await File.ReadAllTextAsync(schemaPath);

        await connection.OpenAsync();
        var trx = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(schema, trx);
        await connection.ExecuteAsync($"PRAGMA SCHEMA_VERSION = {DB_VERSION};", trx);

        await trx.CommitAsync();
        await connection.CloseAsync();

    }

    private async Task<int> GetDatabaseVersion(SqliteConnection connection) {
        return await connection.QuerySingleAsync<int>("PRAGMA schema_version;");
    }

}