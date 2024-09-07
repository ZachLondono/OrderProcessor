using Dapper;
using Domain.Infrastructure.Data;
using Domain.Orders.Persistance;
using Microsoft.Data.Sqlite;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public class TestFileOrderingConnectionFactory(string filePath) : IOrderingDbConnectionFactory {

    private static readonly SemaphoreSlim s_semaphore = new(1);
    private readonly string _schemaFilePath = "./Application/Schemas/ordering_schema.sql";
    private readonly string _filePath = filePath;

    public Task<ISynchronousDbConnection> CreateConnection() {

        s_semaphore.Wait();
        try {

            var builder = new SqliteConnectionStringBuilder {
                DataSource = _filePath,
                Pooling = false,
                Cache = SqliteCacheMode.Shared
            };

            var connection = new SqliteConnection(builder.ConnectionString);

            if (!File.Exists(_filePath)) {
                InitializeDatabase(connection, _schemaFilePath);
            }

            return Task.FromResult((ISynchronousDbConnection) new SynchronousSQLiteDbConnection(connection));

        } finally {

            s_semaphore.Release();

        }

    }

    private static void InitializeDatabase(SqliteConnection connection, string schemaFilePath) {

        var schema = File.ReadAllText(schemaFilePath);
        connection.Execute(schema);

    }

}
