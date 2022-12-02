using System.Data;

namespace ApplicationCore.Features.CADCode.Services;

public interface ICADCodeInventoryDataBaseConnectionFactory {

    public IDbConnection CreateConnection(string filePath);

}