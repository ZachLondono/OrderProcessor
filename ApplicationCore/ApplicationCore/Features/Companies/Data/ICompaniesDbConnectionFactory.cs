using System.Data;

namespace ApplicationCore.Features.Companies.Data;

public interface ICompaniesDbConnectionFactory {

    public Task<IDbConnection> CreateConnection();

}
