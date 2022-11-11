using System.Data;
using System.Data.OleDb;

namespace ApplicationCore.Features.CADCode.Services;

internal class AccessCADCodeLabelDataBaseConnectionStringFactory : ICADCodeLabelDataBaseConnectionStringFactory {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public IDbConnection CreateConnection(string filePath) => new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;data source={filePath}");

}
