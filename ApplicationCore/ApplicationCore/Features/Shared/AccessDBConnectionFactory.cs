using System.Data.OleDb;

namespace ApplicationCore.Features.Shared;

internal class AccessDBConnectionFactory : IAccessDBConnectionFactory {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public OleDbConnection CreateConnection(string filePath) => new($"Provider=Microsoft.ACE.OLEDB.12.0;data source={filePath}");

}
