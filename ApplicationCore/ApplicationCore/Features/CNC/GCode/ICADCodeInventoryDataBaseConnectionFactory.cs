using System.Data;

namespace ApplicationCore.Features.CNC.GCode;

public interface ICADCodeInventoryDataBaseConnectionFactory
{

    public IDbConnection CreateConnection(string filePath);

}