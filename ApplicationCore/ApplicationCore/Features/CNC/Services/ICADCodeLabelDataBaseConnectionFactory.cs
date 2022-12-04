using System.Data;

namespace ApplicationCore.Features.CNC.Services;

internal interface ICADCodeLabelDataBaseConnectionFactory {

    public IDbConnection CreateConnection(string filePath);

}
