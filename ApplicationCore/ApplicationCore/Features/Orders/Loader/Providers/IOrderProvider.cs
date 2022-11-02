using ApplicationCore.Features.Orders.Domain;

namespace ApplicationCore.Features.Orders.Providers;

public interface IOrderProvider {

    public Task<Order?> LoadOrderData();

}
