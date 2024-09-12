using Domain.Infrastructure.Data;
using Domain.Orders.Persistance;
using Microsoft.Data.Sqlite;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

public class TestOrderingConnectionFactory : IOrderingDbConnectionFactory, IDisposable {

    private SynchronousSQLiteDbConnection? _connection = null;
    private readonly string _schemaFilePath;
    private readonly string _dbFilePath;

    public TestOrderingConnectionFactory(string schemaFilePath) {
        _schemaFilePath = schemaFilePath;
        _dbFilePath = Path.GetTempFileName();
    }

    public Task<ISynchronousDbConnection> CreateConnection() {

        if (_connection is null) {

            var schema = File.ReadAllText(_schemaFilePath);

            var builder = new SqliteConnectionStringBuilder {
                DataSource = _dbFilePath,
                Pooling = false,
                Cache = SqliteCacheMode.Shared
            };

            var connection = new SqliteConnection(builder.ConnectionString);

            _connection = new(connection);

            _connection.Execute(schema);

        }

        return Task.FromResult((ISynchronousDbConnection)_connection);

    }

    public void Dispose() {
        File.Delete(_dbFilePath);
    }

}
