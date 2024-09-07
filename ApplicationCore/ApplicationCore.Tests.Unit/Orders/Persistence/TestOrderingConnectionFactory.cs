using Domain.Infrastructure.Data;
using Domain.Orders.Persistance;
using Microsoft.Data.Sqlite;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

internal class TestOrderingConnectionFactory : IOrderingDbConnectionFactory {

    private TestOrderingConnection? _connection = null;
    private readonly string _schemaFilePath;

    public TestOrderingConnectionFactory(string schemaFilePath) {
        _schemaFilePath = schemaFilePath;
    }

    public Task<ISynchronousDbConnection> CreateConnection() {

        if (_connection is null) {

            var schema = File.ReadAllText(_schemaFilePath);

            var connection = new SqliteConnection("Data Source=:memory:");

            _connection = new(connection);

            _connection.Execute(schema);

        }

        return Task.FromResult((ISynchronousDbConnection)_connection);

    }

}
