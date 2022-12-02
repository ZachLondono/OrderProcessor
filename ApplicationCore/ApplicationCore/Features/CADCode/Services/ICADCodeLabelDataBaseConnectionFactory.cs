using System.Data;

namespace ApplicationCore.Features.CADCode.Services;

internal interface ICADCodeLabelDataBaseConnectionFactory {

    public IDbConnection CreateConnection(string filePath);

}
