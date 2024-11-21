using Domain.Orders.Persistance;

namespace Domain.Services.WorkingDirectory;

public class WorkingDirectoryService {

    private readonly IOrderingDbConnectionFactory _connectionFactory;

    public WorkingDirectoryService(IOrderingDbConnectionFactory connectionFactory) {
        _connectionFactory = connectionFactory;
    }

    public async Task<WorkingDirectoryStructure> GetWorkingDirectory(Guid orderId) {

        var workingDirectory = await GetOrderWorkingDirectory(orderId);

        return WorkingDirectoryStructure.Create(workingDirectory);

    }
    
    private async Task<string> GetOrderWorkingDirectory(Guid orderId) {

        using var connection = await _connectionFactory.CreateConnection();

        return connection.QuerySingle<string>("SELECT working_directory FROM orders WHERE id = @OrderId", new { OrderId = orderId });

    }

}
