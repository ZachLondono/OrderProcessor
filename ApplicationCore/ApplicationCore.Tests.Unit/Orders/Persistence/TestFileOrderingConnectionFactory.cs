using Dapper;
using Domain.Orders.Persistance;
using Microsoft.Data.Sqlite;
using System.Data;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public class TestFileOrderingConnectionFactory(string filePath) : IOrderingDbConnectionFactory {

    private static readonly SemaphoreSlim s_semaphore = new(1);
    private readonly string _schemaFilePath = "./Application/Schemas/ordering_schema.sql";
    private readonly string _filePath = filePath;

    public async Task<IDbConnection> CreateConnection() {

        s_semaphore.Wait();
        try {

            var builder = new SqliteConnectionStringBuilder {
                DataSource = _filePath,
                Pooling = false,
                Cache = SqliteCacheMode.Shared
            };

            var connection = new SqliteConnection(builder.ConnectionString);

            if (!File.Exists(_filePath)) {
                await InitializeDatabase(connection, _schemaFilePath);
            }

            return connection;

        } finally {

            s_semaphore.Release();

        }

    }

    private static async Task InitializeDatabase(SqliteConnection connection, string schemaFilePath) {

        var schema = await File.ReadAllTextAsync(schemaFilePath);
        await connection.ExecuteAsync(schema);

    }

}
