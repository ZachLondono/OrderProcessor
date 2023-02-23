using ApplicationCore.Features.Orders.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace ApplicationCore.Tests.Unit.Orders.CreatingNewOrder;

internal class TestOrderingConnectionFactory : IOrderingDbConnectionFactory {

    private readonly string _schemaFilePath;

    public TestOrderingConnectionFactory(string schemaFilePath) {
        _schemaFilePath = schemaFilePath;
    }

    public IDbConnection CreateConnection() {

        var schema = File.ReadAllText(_schemaFilePath);

        var connection = new SqliteConnection("Data Source=:memory:");

        connection.Execute(schema);

        return connection;

    }

}