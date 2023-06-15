using System.Data;

namespace ApplicationCore.Shared.Data.Companies;

public interface ICompaniesDbConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
