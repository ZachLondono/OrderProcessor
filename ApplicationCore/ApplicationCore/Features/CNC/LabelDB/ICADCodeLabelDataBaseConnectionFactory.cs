using System.Data;

namespace ApplicationCore.Features.CNC.LabelDB;

internal interface ICADCodeLabelDataBaseConnectionFactory
{

    public IDbConnection CreateConnection(string filePath);

}
