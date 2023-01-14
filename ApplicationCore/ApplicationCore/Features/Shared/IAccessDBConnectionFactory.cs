using System.Data;

namespace ApplicationCore.Features.Shared;

public interface IAccessDBConnectionFactory {

    public IDbConnection CreateConnection(string filePath);

}
