using System.Data.OleDb;

namespace ApplicationCore.Features.Shared;

public interface IAccessDBConnectionFactory {

    public OleDbConnection CreateConnection(string filePath);

}
