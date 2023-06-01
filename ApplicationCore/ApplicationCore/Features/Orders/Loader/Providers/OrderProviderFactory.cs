using ApplicationCore.Features.Orders.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.Loader.Providers;

public class OrderProviderFactory : IOrderProviderFactory {

    private readonly IServiceProvider _serviceProvider;

    public OrderProviderFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public IOrderProvider GetOrderProvider(OrderSourceType source) => source switch {
        OrderSourceType.AllmoxyWebXML => _serviceProvider.GetRequiredService<AllmoxyWebXMLOrderProvider>(),
        OrderSourceType.AllmoxyFileXML => _serviceProvider.GetRequiredService<AllmoxyFileXMLOrderProvider>(),
        OrderSourceType.DoorOrder => _serviceProvider.GetRequiredService<DoorSpreadsheetOrderProvider>(),
        _ => throw new KeyNotFoundException("No valid order provider for given order source type"),
    };

}
