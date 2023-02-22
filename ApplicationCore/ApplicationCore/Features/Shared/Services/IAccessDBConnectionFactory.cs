using System.Data.OleDb;

namespace ApplicationCore.Features.Shared.Services;

public interface IAccessDBConnectionFactory {

    public OleDbConnection CreateConnection(string filePath);

}
