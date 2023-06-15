using System.Data;

namespace ApplicationCore.Features.Shared.Data.Companies;

public interface ICompaniesDbConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
