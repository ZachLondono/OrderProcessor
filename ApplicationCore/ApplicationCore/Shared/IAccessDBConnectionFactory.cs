using System.Data;

namespace ApplicationCore.Shared;

public interface IAccessDBConnectionFactory {

    public IDbConnection CreateConnection(string filePath);

}
