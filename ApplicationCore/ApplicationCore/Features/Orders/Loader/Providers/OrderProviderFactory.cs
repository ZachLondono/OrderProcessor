using ApplicationCore.Features.Orders.Loader;
using ApplicationCore.Features.Orders.Loader.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.Providers;

public class OrderProviderFactory : IOrderProviderFactory {

    private readonly IServiceProvider _serviceProvider;

    public OrderProviderFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public OrderProvider GetOrderProvider(OrderSourceType source) => source switch {
        OrderSourceType.AllmoxyXML => _serviceProvider.GetRequiredService<AllmoxyXMLOrderProvider>(),
        _ => throw new KeyNotFoundException("No valid order provider for given order source type"),
    };

}
