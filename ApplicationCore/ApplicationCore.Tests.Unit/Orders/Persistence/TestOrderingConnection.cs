using Domain.Infrastructure.Data;
using System.Data;

namespace ApplicationCore.Tests.Unit.Orders.Persistence;

internal class TestOrderingConnection : ISynchronousDbConnection {

    private readonly IDbConnection _connection;

    public TestOrderingConnection(IDbConnection connection) {
        _connection = connection;
    }

    public string? ConnectionString {
        get => _connection.ConnectionString;
        set => _connection.ConnectionString = value;
    }

    public int ConnectionTimeout => _connection.ConnectionTimeout;

    public string Database => _connection.Database;

    public ConnectionState State => _connection.State;

    public ISynchronousDbTransaction BeginTransaction() {
        return new SynchronousDbTransaction(_connection.BeginTransaction());
    }

    public void Close() {
    }

    public void Dispose() {
    }

    public void Open() {
        _connection.Open();
    }

    public int Execute(string sql, object? param = null, ISynchronousDbTransaction? transaction = null) {
        throw new NotImplementedException();
    }

    public T? QueryFirstOrDefault<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null) {
        throw new NotImplementedException();
    }

    public T? QuerySingleOrDefault<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null) {
        throw new NotImplementedException();
    }

    public T QuerySingle<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null) {
        throw new NotImplementedException();
    }

    public IEnumerable<T> Query<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null) {
        throw new NotImplementedException();
    }

    public IEnumerable<dynamic> Query(string sql, object? param = null, ISynchronousDbTransaction? transaction = null) {
        throw new NotImplementedException();
    }
}