using Dapper;
using Microsoft.Data.Sqlite;

namespace Domain.Infrastructure.Data;

public class SynchronousSQLiteDbConnection : ISynchronousDbConnection {

    private readonly SqliteConnection _connection;

    public SynchronousSQLiteDbConnection(SqliteConnection connection) {
        _connection = connection;
    }

    public void Open() => new NotImplementedException();
    public void Close() => _connection.Close();

    public ISynchronousDbTransaction BeginTransaction() => new SynchronousDbTransaction(_connection.BeginTransaction());

    public int Execute(string sql, object? param = null, ISynchronousDbTransaction? transaction = null)
        => _connection.Execute(sql, param, transaction?.GetDbTransaction());

    public T? QueryFirstOrDefault<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null)
        => _connection.QueryFirstOrDefault<T>(sql, param, transaction?.GetDbTransaction());

    public T? QuerySingleOrDefault<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null)
        => _connection.QuerySingleOrDefault<T>(sql, param, transaction?.GetDbTransaction());

    public T QuerySingle<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null)
        => _connection.QuerySingle<T>(sql, param, transaction?.GetDbTransaction());

    public IEnumerable<T> Query<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null)
        => _connection.Query<T>(sql, param, transaction?.GetDbTransaction());

    public void Dispose() => _connection.Dispose();

    public IEnumerable<dynamic> Query(string sql, object? param = null, ISynchronousDbTransaction? transaction = null)
        => _connection.Query(sql, param, transaction?.GetDbTransaction());

}
