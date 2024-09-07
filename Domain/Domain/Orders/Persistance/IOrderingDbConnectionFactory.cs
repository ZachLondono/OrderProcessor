using Domain.Infrastructure.Data;

namespace Domain.Orders.Persistance;

public interface IOrderingDbConnectionFactory {

    public Task<ISynchronousDbConnection> CreateConnection();

}