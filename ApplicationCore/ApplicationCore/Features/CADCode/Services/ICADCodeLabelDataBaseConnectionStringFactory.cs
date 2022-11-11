using System.Data;

namespace ApplicationCore.Features.CADCode.Services;

internal interface ICADCodeLabelDataBaseConnectionStringFactory {

    public IDbConnection CreateConnection(string filePath);

}
