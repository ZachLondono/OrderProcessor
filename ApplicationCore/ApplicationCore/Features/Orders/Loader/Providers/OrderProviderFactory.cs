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
        OrderSourceType.RichelieuXML => _serviceProvider.GetRequiredService<RichelieuXMLOrderProvider>(),
        OrderSourceType.OTExcel => _serviceProvider.GetRequiredService<OTExcelProvider>(),
        OrderSourceType.HafeleExcel => _serviceProvider.GetRequiredService<HafeleExcelProvider>(),
        _ => throw new NotImplementedException(),
    };

}
