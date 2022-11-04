namespace ApplicationCore.Features.Orders.Loader.Providers;

public interface IOrderProviderFactory {

    public IOrderProvider GetOrderProvider(OrderSource source);

}
