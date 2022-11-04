using ApplicationCore.Features.Orders.Domain;

namespace ApplicationCore.Features.Orders.Loader.Providers;

public interface IOrderProvider {

    public Task<Order?> LoadOrderData();

}
