using System.Data;

namespace ApplicationCore.Infrastructure.Data;

public interface IDbConnectionFactory {

    public IDbConnection CreateConnection();

}
