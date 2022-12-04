using System.Data;

namespace ApplicationCore.Features.CNC.Services;

public interface ICADCodeInventoryDataBaseConnectionFactory {

    public IDbConnection CreateConnection(string filePath);

}