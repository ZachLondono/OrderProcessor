using ApplicationCore.Features.Orders.Loader;

namespace ApplicationCore.Features.Orders.Providers;

public interface IOrderProviderFactory {

    public IOrderProvider GetOrderProvider(OrderSource source);

}
