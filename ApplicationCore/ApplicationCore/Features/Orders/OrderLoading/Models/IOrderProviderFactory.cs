using ApplicationCore.Features.Orders.OrderLoading;
using OrderLoading;

namespace ApplicationCore.Features.Orders.OrderLoading.Models;

public interface IOrderProviderFactory {

    public IOrderProvider GetOrderProvider(OrderSourceType type);

}
