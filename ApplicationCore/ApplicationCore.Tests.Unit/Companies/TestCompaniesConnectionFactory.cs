using ApplicationCore.Shared.Data.Companies;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace ApplicationCore.Tests.Unit.Companies;

public class TestCompaniesConnectionFactory : ICompaniesDbConnectionFactory {

    private TestCompaniesConnection? _connection = null;
    private readonly string _schemaFilePath;

    public TestCompaniesConnectionFactory(string schemaFilePath) {
        _schemaFilePath = schemaFilePath;
    }

    public Task<IDbConnection> CreateConnection() {

        if (_connection is null) {

            var schema = File.ReadAllText(_schemaFilePath);

            var connection = new SqliteConnection("Data Source=:memory:");

            _connection = new(connection);

            _connection.Execute(schema);

        }

        return Task.FromResult((IDbConnection)_connection);

    }

}
