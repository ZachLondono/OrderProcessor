using System.Data;

namespace Companies.Infrastructure;

public interface ICompaniesDbConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
