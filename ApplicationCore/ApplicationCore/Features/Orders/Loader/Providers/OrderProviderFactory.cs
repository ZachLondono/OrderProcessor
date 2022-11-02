using ApplicationCore.Features.Orders.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationCore.Features.Orders.Providers;

public class OrderProviderFactory : IOrderProviderFactory {

    private readonly IServiceProvider _serviceProvider;

    public OrderProviderFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public IOrderProvider GetOrderProvider(OrderSource source) => source switch {
        OrderSource.AllmoxyXML => _serviceProvider.GetRequiredService<AllmoxyXMLOrderProvider>(),
        _ => throw new NotImplementedException(),
    };

}
