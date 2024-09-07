using Companies.Infrastructure;
using Domain.Infrastructure.Data;
using Microsoft.Data.Sqlite;

namespace Companies.Tests.Unit;

public class TestCompaniesConnectionFactory : ICompaniesDbConnectionFactory {

    private TestCompaniesConnection? _connection = null;
    private readonly string _schemaFilePath;

    public TestCompaniesConnectionFactory(string schemaFilePath) {
        _schemaFilePath = schemaFilePath;
    }

    public Task<ISynchronousDbConnection> CreateConnection() {

        if (_connection is null) {

            var schema = File.ReadAllText(_schemaFilePath);

            var connection = new SqliteConnection("Data Source=:memory:");

            _connection = new(connection);

            _connection.Execute(schema);

        }

        return Task.FromResult((ISynchronousDbConnection) _connection);

    }

}
