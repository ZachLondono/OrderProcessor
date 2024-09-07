using System.Data;

namespace Domain.Infrastructure.Data;

public interface ISynchronousDbTransaction {

    public void Commit();
    public void Rollback();
    internal IDbTransaction GetDbTransaction();

}
