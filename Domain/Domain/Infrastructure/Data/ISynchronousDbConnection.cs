namespace Domain.Infrastructure.Data;

public interface ISynchronousDbConnection : IDisposable {

    public void Open();
    public void Close();
    public ISynchronousDbTransaction BeginTransaction(); // Create wrapper for transaction

    public int Execute(string sql, object? param = null, ISynchronousDbTransaction? transaction = null);
    public T? QueryFirstOrDefault<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null);
    public T? QuerySingleOrDefault<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null);
    public T QuerySingle<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null);
    public IEnumerable<T> Query<T>(string sql, object? param = null, ISynchronousDbTransaction? transaction = null);
    public IEnumerable<dynamic> Query(string sql, object? param = null, ISynchronousDbTransaction? transaction = null);

}
