namespace ApplicationCore.Features.Orders.OrderLoading.Models;

public interface IOrderProviderFactory {

    public IOrderProvider GetOrderProvider(OrderSourceType type);

}
