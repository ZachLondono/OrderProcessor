using System.Data;

namespace Domain.Infrastructure.Data;

public class SynchronousDbTransaction : ISynchronousDbTransaction {

    private readonly IDbTransaction _transaction;

    public SynchronousDbTransaction(IDbTransaction transaction) {
        _transaction = transaction;
    }

    public void Commit() => _transaction.Commit();

    public void Rollback() => _transaction.Rollback();

    IDbTransaction ISynchronousDbTransaction.GetDbTransaction() {
        throw new NotImplementedException();
    }

}
