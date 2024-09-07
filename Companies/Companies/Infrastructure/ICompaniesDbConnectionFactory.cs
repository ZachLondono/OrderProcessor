using Domain.Infrastructure.Data;

namespace Companies.Infrastructure;

public interface ICompaniesDbConnectionFactory {

    public Task<ISynchronousDbConnection> CreateConnection();

}
