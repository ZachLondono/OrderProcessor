using System.Data;

namespace ApplicationCore.Tests.Unit.Companies;

public class TestCompaniesConnection : IDbConnection {

    private readonly IDbConnection _connection;

    public TestCompaniesConnection(IDbConnection connection) {
        _connection = connection;
    }

    public string? ConnectionString {
        get => _connection.ConnectionString;
        set => _connection.ConnectionString = value;
    }

    public int ConnectionTimeout => _connection.ConnectionTimeout;

    public string Database => _connection.Database;

    public ConnectionState State => _connection.State;

    public IDbTransaction BeginTransaction() {
        return _connection.BeginTransaction();
    }

    public IDbTransaction BeginTransaction(IsolationLevel il) {
        return _connection.BeginTransaction(il);
    }

    public void ChangeDatabase(string databaseName) {
        _connection.ChangeDatabase(databaseName);
    }

    public IDbCommand CreateCommand() {
        return _connection.CreateCommand();
    }

    public void Close() {
    }

    public void Dispose() {
    }

    public void Open() {
        _connection.Open();
    }

}
