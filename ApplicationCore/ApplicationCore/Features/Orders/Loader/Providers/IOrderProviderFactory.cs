namespace ApplicationCore.Features.Orders.Loader.Providers;

public interface IOrderProviderFactory {

    public OrderProvider GetOrderProvider(OrderSourceType type);

}
